\ -> (to/is replacement) recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

: post-to, ( nt -- )  swap lit, lit, ;

' (to) ' (to), ' post-to, >postponer translate: translate-to
' translate-to Constant rectype-to

: rec-to ( addr u -- xt n r:to | rectype-null ) \ gforth-experimental
    \G words prefixed with @code{->} are treated as if preceeded by
    \G @code{TO}, with @code{+>} as @code{+TO}, with
    \G @code{'>} as @code{ADDR}, with @code{@@>} as @code{ACTION-OF}, and
    \G with @code{=>} as @code{IS}.
    dup 3 u< IF  2drop ['] notfound  EXIT  THEN
    over 1+ c@ '>' <> IF  2drop ['] notfound  EXIT  THEN
    case  over c@
	'-' of  0  endof
	'+' of  1  endof
	''' of  2  endof
	'@' of  3  endof
	'=' of  4  endof
	drop 2drop ['] notfound  EXIT
    endcase  -rot
    2 /string forth-recognize
    translate-nt? 0= IF  drop ['] notfound EXIT  THEN
    dup >namehm @ >hmto @ ['] no-to = IF  2drop ['] notfound EXIT  THEN
    name>interpret ['] translate-to ;

' rec-to forth-recognizer >back
