static ServerCommandError
server_ptrace_read_data (InferiorHandle *handle, guint64 start, guint32 size, gpointer buffer)
{
	guint8 *ptr = buffer;
	guint32 old_size = size;

	while (size) {
		int ret = pread64 (handle->mem_fd, ptr, size, start);
		if (ret < 0) {
			if (errno == EINTR)
				continue;
			g_warning (G_STRLOC ": Can't read target memory at address %08Lx: %s",
				   start, g_strerror (errno));
			return COMMAND_ERROR_UNKNOWN;
		}

		size -= ret;
		ptr += ret;
	}

	debugger_arch_i386_remove_breakpoints_from_target_memory (handle, start, old_size, buffer);

	return COMMAND_ERROR_NONE;
}

static ServerCommandError
server_ptrace_set_dr (InferiorHandle *handle, int regnum, unsigned long value)
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
do_wait (InferiorHandle *handle)
{
	int ret, status = 0;
	sigset_t mask, oldmask;

	sigemptyset (&mask);
	sigaddset (&mask, SIGCHLD);
	sigaddset (&mask, SIGINT);

	sigprocmask (SIG_BLOCK, &mask, &oldmask);

 again:
	ret = waitpid (handle->pid, &status, WUNTRACED | WNOHANG | __WALL | __WCLONE);
	if (ret < 0) {
		g_warning (G_STRLOC ": Can't waitpid (%d): %s", handle->pid, g_strerror (errno));
		status = -1;
		goto out;
	} else if (ret) {
		goto out;
	}

	sigsuspend (&oldmask);
	goto again;

 out:
	sigprocmask (SIG_SETMASK, &oldmask, NULL);
	return status;
}

static void
setup_inferior (InferiorHandle *handle)
{
	gchar *filename = g_strdup_printf ("/proc/%d/mem", handle->pid);
	sigset_t mask;

	sigemptyset (&mask);
	sigaddset (&mask, SIGINT);
	pthread_sigmask (SIG_BLOCK, &mask, NULL);

	do_wait (handle);

	handle->mem_fd = open64 (filename, O_RDONLY);

	if (handle->mem_fd < 0)
		g_error (G_STRLOC ": Can't open (%s): %s", filename, g_strerror (errno));

	g_free (filename);

	if (get_registers (handle, &handle->current_regs) != COMMAND_ERROR_NONE)
		g_error (G_STRLOC ": Can't get registers");
	if (get_fp_registers (handle, &handle->current_fpregs) != COMMAND_ERROR_NONE)
		g_error (G_STRLOC ": Can't get fp registers");
}
