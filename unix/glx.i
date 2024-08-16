// this file is in the public domain
%module glx
%insert("include")
%{
#define GLX_GLXEXT_PROTOTYPES
#include <GL/glx.h>
%}
%apply int { XID, Bool, GLsizei, Pixmap, Font, Window };
%apply long long { int64_t };
%apply float { GLfloat };

// exec: sed -e 's/^c-library\( .*\)/[IFUNDEF] opengl cs-vocabulary opengl [THEN]``get-current also opengl definitions``c-library\1`/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

#define SWIG_FORTH_GFORTH_LIBRARY "GLX"

%include <GL/glx.h>
