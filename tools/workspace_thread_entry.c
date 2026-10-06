#define _GNU_SOURCE
#include <dlfcn.h>
#include <pthread.h>
#include <stdlib.h>

/* This workspace lacks /proc, so CoreCLR cannot determine the main stack.
 * A pthread has explicit stack metadata available through pthread_getattr_np.
 * Run the real main on that thread without changing application behavior. */
static int (*application_main)(int, char **, char **);
struct arguments { int argc; char **argv; char **envp; int result; };
static void *thread_main(void *value) {
    struct arguments *args = value;
    args->result = application_main(args->argc, args->argv, args->envp);
    return NULL;
}
static int wrapped_main(int argc, char **argv, char **envp) {
    struct arguments args = {argc, argv, envp, 1};
    pthread_t thread;
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setstacksize(&attr, 16 * 1024 * 1024);
    int error = pthread_create(&thread, &attr, thread_main, &args);
    pthread_attr_destroy(&attr);
    if (error) return 125;
    pthread_join(thread, NULL);
    return args.result;
}
int __libc_start_main(int (*main_fn)(int, char **, char **), int argc,
                     char **argv, void (*init)(void), void (*fini)(void),
                     void (*rtld_fini)(void), void *stack_end) {
    typedef int (*start_fn)(int (*)(int, char **, char **), int, char **,
                           void (*)(void), void (*)(void), void (*)(void), void *);
    start_fn real_start = (start_fn)dlsym(RTLD_NEXT, "__libc_start_main");
    application_main = main_fn;
    return real_start(wrapped_main, argc, argv, init, fini, rtld_fini, stack_end);
}
