/* Build-host adapter only. Never package this library with a desktop export.
 * Redirect the absent /tmp namespace to owned workspace storage; keep real
 * shared-memory files and native locks. The caller supplies minimal process
 * metadata for SDK startup, not a general /proc emulation. */
#define _GNU_SOURCE
#include <dlfcn.h>
#include <dirent.h>
#include <fcntl.h>
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/stat.h>
#include <errno.h>
static const char *map_path(const char *p,char *buf,size_t size){
 const char *root=getenv("OPENGDS2_SDK_STATE");if(!root || !p)return p;
 if(!strcmp(p,"/tmp") || !strncmp(p,"/tmp/",5)){int n=snprintf(buf,size,"%s/tmp%s",root,p+4);if(n<0 || (size_t)n>=size){errno=ENAMETOOLONG;return NULL;}return buf;}
 if(!strncmp(p,"/proc/",6) && strstr(p,"/stat") && !strcmp(strstr(p,"/stat"),"/stat")){int n=snprintf(buf,size,"%s/process.stat",root);if(n<0 || (size_t)n>=size){errno=ENAMETOOLONG;return NULL;}return buf;}
 return p;
}
#define P char b[4096];const char *mapped=map_path(path,b,sizeof(b))
int open64(const char *path,int flags,...){P;mode_t mode=0;if(flags&O_CREAT){va_list a;va_start(a,flags);mode=va_arg(a,int);va_end(a);}int(*real)(const char*,int,...)=dlsym(RTLD_NEXT,"open64");return real(mapped,flags,mode);}
int open(const char *path,int flags,...){P;mode_t mode=0;if(flags&O_CREAT){va_list a;va_start(a,flags);mode=va_arg(a,int);va_end(a);}int(*real)(const char*,int,...)=dlsym(RTLD_NEXT,"open");return real(mapped,flags,mode);}
int stat(const char *path,struct stat *s){P;int(*r)(const char*,struct stat*)=dlsym(RTLD_NEXT,"stat");return r(mapped,s);}
int stat64(const char *path,struct stat64 *s){P;int(*r)(const char*,struct stat64*)=dlsym(RTLD_NEXT,"stat64");return r(mapped,s);}
int __xstat64(int v,const char *path,struct stat64 *s){P;int(*r)(int,const char*,struct stat64*)=dlsym(RTLD_NEXT,"__xstat64");return r(v,mapped,s);}
int __lxstat64(int v,const char *path,struct stat64 *s){P;int(*r)(int,const char*,struct stat64*)=dlsym(RTLD_NEXT,"__lxstat64");return r(v,mapped,s);}
int mkdir(const char *path,mode_t mode){P;int(*r)(const char*,mode_t)=dlsym(RTLD_NEXT,"mkdir");return r(mapped,mode);}
int chmod(const char *path,mode_t mode){P;int(*r)(const char*,mode_t)=dlsym(RTLD_NEXT,"chmod");return r(mapped,mode);}
int access(const char *path,int mode){P;int(*r)(const char*,int)=dlsym(RTLD_NEXT,"access");return r(mapped,mode);}
int unlink(const char *path){P;int(*r)(const char*)=dlsym(RTLD_NEXT,"unlink");return r(mapped);}
int rmdir(const char *path){P;int(*r)(const char*)=dlsym(RTLD_NEXT,"rmdir");return r(mapped);}
int chdir(const char *path){P;int(*r)(const char*)=dlsym(RTLD_NEXT,"chdir");return r(mapped);}
DIR *opendir(const char *path){P;DIR*(*r)(const char*)=dlsym(RTLD_NEXT,"opendir");return r(mapped);}
FILE *fopen64(const char *path,const char *mode){P;FILE*(*r)(const char*,const char*)=dlsym(RTLD_NEXT,"fopen64");return r(mapped,mode);}
FILE *fopen(const char *path,const char *mode){P;FILE*(*r)(const char*,const char*)=dlsym(RTLD_NEXT,"fopen");return r(mapped,mode);}
int rename(const char *a,const char *c){char b[4096],d[4096];int(*r)(const char*,const char*)=dlsym(RTLD_NEXT,"rename");return r(map_path(a,b,sizeof(b)),map_path(c,d,sizeof(d)));}
char *mkdtemp(char *path){char b[4096];const char *mapped=map_path(path,b,sizeof(b));char*(*r)(char*)=dlsym(RTLD_NEXT,"mkdtemp");if(mapped==path)return r(path);if(!r(b))return NULL;size_t n=strlen(path),m=strlen(b);memcpy(path+n-6,b+m-6,6);return path;}
int mkstemp(char *path){char b[4096];const char *mapped=map_path(path,b,sizeof(b));int(*r)(char*)=dlsym(RTLD_NEXT,"mkstemp");if(!mapped)return -1;if(mapped==path)return r(path);int fd=r(b);if(fd>=0){size_t n=strlen(path),m=strlen(b);memcpy(path+n-6,b+m-6,6);}return fd;}
int mkstemp64(char *path){return mkstemp(path);}
