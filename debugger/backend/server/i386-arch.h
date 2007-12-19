#ifndef __MONO_DEBUGGER_I386_ARCH_H__
#define __MONO_DEBUGGER_I386_ARCH_H__

#include <glib.h>

G_BEGIN_DECLS

#if defined(__i386__)

#include <asm/user.h>

#define INFERIOR_REGS_TYPE	struct user_regs_struct
#define INFERIOR_FPREGS_TYPE	struct user_i387_struct

#define INFERIOR_REG_EIP(r)	r.eip
#define INFERIOR_REG_ESP(r)	r.esp
#define INFERIOR_REG_EBP(r)	r.ebp
#define INFERIOR_REG_EAX(r)	r.eax
#define INFERIOR_REG_EBX(r)	r.ebx
#define INFERIOR_REG_ECX(r)	r.ecx
#define INFERIOR_REG_EDX(r)	r.edx
#define INFERIOR_REG_ESI(r)	r.esi
#define INFERIOR_REG_EDI(r)	r.edi
#define INFERIOR_REG_EFLAGS(r)	r.eflags
#define INFERIOR_REG_ESP(r)	r.esp
#define INFERIOR_REG_FS(r)	r.fs
#define INFERIOR_REG_ES(r)	r.es
#define INFERIOR_REG_DS(r)	r.ds
#define INFERIOR_REG_CS(r)	r.cs
#define INFERIOR_REG_SS(r)	r.ss
#define INFERIOR_REG_GS(r)	r.gs

G_END_DECLS

#else
#error "Wrong architecture!"
#endif

#endif