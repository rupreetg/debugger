#include <mono-debugger-jit-wrapper.h>
#include <mono/io-layer/io-layer.h>
#include <mono/metadata/threads.h>
#define IN_MONO_DEBUGGER
#include <mono/private/libgc-mono-debugger.h>
#include <unistd.h>
#include <string.h>

static sem_t thread_manager_cond;
static sem_t thread_manager_finished_cond;
static CRITICAL_SECTION thread_manager_finished_mutex;
static CRITICAL_SECTION thread_manager_mutex;
static GPtrArray *thread_array = NULL;

static void (*notification_function) (int tid, gpointer data);

int mono_debugger_thread_manager_notify_command = 0;
int mono_debugger_thread_manager_notify_tid = 0;

extern void GC_push_all_stack (gpointer b, gpointer t);

void
mono_debugger_thread_manager_main (void)
{
	notification_function (0, NULL);

	while (TRUE) {
		/* Wait for an event. */
		IO_LAYER (LeaveCriticalSection) (&thread_manager_mutex);
		mono_debugger_wait_cond (&thread_manager_cond);
		IO_LAYER (EnterCriticalSection) (&thread_manager_mutex);

		/*
		 * Send notification - we'll stop on a breakpoint instruction at a special
		 * address.  The debugger will reload the thread list while we're stopped -
		 * and owning the `thread_manager_mutex' so that no other thread can touch
		 * them in the meantime.
		 */
		notification_function (0, NULL);

		sem_post (&thread_manager_finished_cond);
	}
}

static void
debugger_gc_stop_world (void)
{
	mono_debugger_thread_manager_acquire_global_thread_lock ();
}

static void
debugger_gc_start_world (void)
{
	mono_debugger_thread_manager_release_global_thread_lock ();
}

static void
debugger_gc_push_all_stacks (void)
{
	int i, tid;

	tid = IO_LAYER (GetCurrentThreadId) ();

	if (!thread_array)
		return;

	for (i = 0; i < thread_array->len; i++) {
		MonoDebuggerThread *thread = g_ptr_array_index (thread_array, i);
		gpointer end_stack = (thread->tid == tid) ? &i : thread->end_stack;

		GC_push_all_stack (end_stack, thread->start_stack);
	}
}

GCThreadFunctions mono_debugger_thread_vtable = {
	NULL,

	debugger_gc_stop_world,
	debugger_gc_push_all_stacks,
	debugger_gc_start_world
};

void
mono_debugger_thread_manager_init (void)
{
	IO_LAYER (InitializeCriticalSection) (&thread_manager_mutex);
	IO_LAYER (InitializeCriticalSection) (&thread_manager_finished_mutex);
	sem_init (&thread_manager_cond, 0, 0);
	sem_init (&thread_manager_finished_cond, 0, 0);

	if (!thread_array)
		thread_array = g_ptr_array_new ();

	gc_thread_vtable = &mono_debugger_thread_vtable;

	notification_function = mono_debugger_create_notification_function (
		(gpointer) &MONO_DEBUGGER__manager.thread_manager_notification);

	IO_LAYER (EnterCriticalSection) (&thread_manager_mutex);
}

static void
signal_thread_manager (guint32 command, guint32 tid)
{
	IO_LAYER (EnterCriticalSection) (&thread_manager_mutex);
	mono_debugger_thread_manager_notify_command = command;
	mono_debugger_thread_manager_notify_tid = tid;
	sem_post (&thread_manager_cond);
	IO_LAYER (LeaveCriticalSection) (&thread_manager_mutex);

	mono_debugger_wait_cond (&thread_manager_finished_cond);
	mono_debugger_thread_manager_notify_command = 0;
	mono_debugger_thread_manager_notify_tid = 0;
}

void
mono_debugger_thread_manager_add_thread (guint32 tid, gpointer start_stack, gpointer func)
{
	MonoDebuggerThread *thread = g_new0 (MonoDebuggerThread, 1);

	thread->tid = tid;
	thread->func = func;
	thread->start_stack = start_stack;

	IO_LAYER (EnterCriticalSection) (&thread_manager_finished_mutex);

	notification_function (tid, func);

	mono_debugger_thread_manager_thread_created (thread);

	IO_LAYER (LeaveCriticalSection) (&thread_manager_finished_mutex);
}

void
mono_debugger_thread_manager_thread_created (MonoDebuggerThread *thread)
{
	if (!thread_array)
		thread_array = g_ptr_array_new ();

	g_ptr_array_add (thread_array, thread);
}

void
mono_debugger_thread_manager_start_resume (guint32 tid)
{
}

void
mono_debugger_thread_manager_end_resume (guint32 tid)
{
}

void
mono_debugger_thread_manager_acquire_global_thread_lock (void)
{
	int tid = IO_LAYER (GetCurrentThreadId) ();

	IO_LAYER (EnterCriticalSection) (&thread_manager_finished_mutex);

	signal_thread_manager (THREAD_MANAGER_ACQUIRE_GLOBAL_LOCK, tid);

	IO_LAYER (LeaveCriticalSection) (&thread_manager_finished_mutex);
}

void
mono_debugger_thread_manager_release_global_thread_lock (void)
{
	int tid = IO_LAYER (GetCurrentThreadId) ();

	IO_LAYER (EnterCriticalSection) (&thread_manager_finished_mutex);

	signal_thread_manager (THREAD_MANAGER_RELEASE_GLOBAL_LOCK, tid);

	IO_LAYER (LeaveCriticalSection) (&thread_manager_finished_mutex);
}
