ServerCommandError
_mono_debugger_server_get_registers (InferiorHandle *inferior, INFERIOR_REGS_TYPE *regs)
{
	if (ptrace (PT_GETREGS, inferior->pid, NULL, regs) != 0) {
		if (errno == ESRCH)
			return COMMAND_ERROR_NOT_STOPPED;
		else if (errno) {
			g_message (G_STRLOC ": %d - %s", inferior->pid, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}
	}

	return COMMAND_ERROR_NONE;
}

ServerCommandError
_mono_debugger_server_set_registers (InferiorHandle *inferior, INFERIOR_REGS_TYPE *regs)
{
	if (ptrace (PT_SETREGS, inferior->pid, NULL, regs) != 0) {
		if (errno == ESRCH)
			return COMMAND_ERROR_NOT_STOPPED;
		else if (errno) {
			g_message (G_STRLOC ": %d - %s", inferior->pid, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}
	}

	return COMMAND_ERROR_NONE;
}

ServerCommandError
_mono_debugger_server_get_fp_registers (InferiorHandle *inferior, INFERIOR_FPREGS_TYPE *regs)
{
	if (ptrace (PT_GETFPREGS, inferior->pid, NULL, regs) != 0) {
		if (errno == ESRCH)
			return COMMAND_ERROR_NOT_STOPPED;
		else if (errno) {
			g_message (G_STRLOC ": %d - %s", inferior->pid, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}
	}

	return COMMAND_ERROR_NONE;
}

ServerCommandError
_mono_debugger_server_set_fp_registers (InferiorHandle *inferior, INFERIOR_FPREGS_TYPE *regs)
{
	if (ptrace (PT_SETFPREGS, inferior->pid, NULL, regs) != 0) {
		if (errno == ESRCH)
			return COMMAND_ERROR_NOT_STOPPED;
		else if (errno) {
			g_message (G_STRLOC ": %d - %s", inferior->pid, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}
	}

	return COMMAND_ERROR_NONE;
}

ServerCommandError
mono_debugger_server_read_memory (ServerHandle *handle, guint64 start,
				  guint32 size, gpointer buffer)
{
	guint8 *ptr = buffer;
	guint32 old_size = size;

	while (size) {
		int ret = pread64 (handle->inferior->mem_fd, ptr, size, start);
		if (ret < 0) {
			if (errno == EINTR)
				continue;
			else if (errno == EIO)
				return COMMAND_ERROR_MEMORY_ACCESS;
			g_warning (G_STRLOC ": Can't read target memory at address %08Lx: %s",
				   start, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}

		size -= ret;
		ptr += ret;
	}

	i386_arch_remove_breakpoints_from_target_memory (handle, start, old_size, buffer);

	return COMMAND_ERROR_NONE;
}

ServerCommandError
_mono_debugger_server_set_dr (InferiorHandle *handle, int regnum, unsigned long value)
{
	errno = 0;
	ptrace (PTRACE_POKEUSER, handle->pid, offsetof (struct user, u_debugreg [regnum]), value);
	if (errno) {
		g_message (G_STRLOC ": %d - %d - %s", handle->pid, regnum, g_strerror (errno));
		return COMMAND_ERROR_UNKNOWN;
	}

	return COMMAND_ERROR_NONE;
}

static int
do_wait (guint32 *status)
{
	int ret;

	ret = waitpid (-1, status, WUNTRACED | WNOHANG | __WALL | __WCLONE);
	if (ret < 0) {
		g_warning (G_STRLOC ": Can't waitpid: %s", g_strerror (errno));
		return -1;
	} else if (ret)
		return ret;

	GC_start_blocking ();
	ret = waitpid (-1, status, WUNTRACED | __WALL | __WCLONE);
	GC_end_blocking ();
	if (ret < 0) {
		g_warning (G_STRLOC ": Can't waitpid: %s", g_strerror (errno));
		return -1;
	}

	return ret;
}

guint32
mono_debugger_server_wait (guint64 *status_ret)
{
	int ret, status;

	ret = do_wait (&status);
	if (ret < 0)
		return -1;

	*status_ret = status;
	return ret;
}

void
_mono_debugger_server_setup_inferior (ServerHandle *handle)
{
	gchar *filename = g_strdup_printf ("/proc/%d/mem", handle->inferior->pid);

	handle->inferior->mem_fd = open64 (filename, O_RDONLY);

	if (handle->inferior->mem_fd < 0)
		g_error (G_STRLOC ": Can't open (%s): %s", filename, g_strerror (errno));

	g_free (filename);
}

gboolean
_mono_debugger_server_setup_thread_manager (ServerHandle *handle)
{
	int flags = PTRACE_O_TRACEFORK | PTRACE_O_TRACEVFORKDONE | PTRACE_O_TRACECLONE;

	if (ptrace (PTRACE_SETOPTIONS, handle->inferior->pid, 0, flags))
		return FALSE;

	return TRUE;
}

ServerCommandError
mono_debugger_server_get_signal_info (ServerHandle *handle, SignalInfo *sinfo)
{
	sinfo->sigkill = SIGKILL;
	sinfo->sigstop = SIGSTOP;
	sinfo->sigint = SIGINT;
	sinfo->sigchld = SIGCHLD;
	sinfo->sigprof = SIGPROF;
	sinfo->sigpwr = SIGPWR;
	sinfo->sigxcpu = SIGXCPU;

#if 0
	sinfo->thread_abort = 34;
	sinfo->thread_restart = 33;
	sinfo->thread_debug = 32;
	sinfo->mono_thread_debug = -1;
#else
	sinfo->thread_abort = 33;
	sinfo->thread_restart = 32;
	sinfo->thread_debug = 34;
	sinfo->mono_thread_debug = 34;
#endif

	return COMMAND_ERROR_NONE;
}
