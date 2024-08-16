\ paths.fs path file handling                                    03may97jaw

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2010,2013,2016,2017,2018,2019,2021,2022,2023 Free Software Foundation, Inc.

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

\ -Changing the search-path:
\ fpath+ <path> 	adds a directory to the searchpath
\ fpath= <path>|<path>	makes complete now searchpath
\ 			seperator is |
\ .fpath		displays the search path
\ remark I: 
\ a ./ in the beginning of filename is expanded to the directory the
\ current file comes from. ./ can also be included in the search-path!
\ ~+/ loads from the current working directory

\ remark II:
\ if there is no sufficient space for the search path increase it!


\ -Creating custom paths:

\ It is possible to use the search mechanism on yourself.

\ Make a buffer for the path:
\ create mypath	100 path,
\ mypath path+ 
\ mypath path=
\ mypath .path

\ do a open with the search path:
\ open-path-file ( adr len path -- fd adr len ior )
\ the file is opened read-only; if the file is not found an error is generated

\ questions to: wilke@jwdt.com

include stringk.fs

Variable fpath ( -- path-addr ) \ gforth
User ofile
User tfile

: clear-paths ( -- )
    fpath off
    ofile off
    tfile off ;
: os-cold ( -- )
    clear-paths pathstring 2@ fpath only-path ;

\ The path Gforth uses for @code{included} and friends.

: also-path ( c-addr len path-addr -- ) \ gforth
    \G add the directory @i{c-addr len} to @i{path-addr}.
    dup $@len IF  \ add separator if necessary
	0 over c$+!
    THEN
    $+! ;

: clear-path ( path-addr -- ) \ gforth
    \G Set the path @i{path-addr} to empty.
    $init ;

: only-path ( adr len path -- )
    dup clear-path also-path ;

: path+ ( path-addr  "dir" -- ) \ gforth
    \G Add the directory @var{dir} to the search path @var{path-addr}.
    ?parse-name rot also-path ;

: fpath+ ( "dir" ) \ gforth
    \G Add directory @var{dir} to the Forth search path.
    fpath path+ ;

: substc ( addr u charold charnew -- addr u )
    2over bounds ?DO over i c@ = IF dup i c! THEN LOOP  2drop ;

: path= ( path-addr "dir1|dir2|dir3" -- ) \ gforth path-equals
    \G Make a complete new search path; the path separator is |.
    ?parse-name '|' 0 substc rot only-path ;

: fpath= ( "dir1|dir2|dir3" ) \ gforth f-path-equals
    \G Make a complete new Forth search path; the path separator is |.
    fpath path= ;

: path>string ( path -- c-addr u )
    \ string contains NULs to separate/terminate components
    $@ ;

: next-path ( addr u -- addr1 u1 addr2 u2 )
    \ addr2 u2 is the first component of the path, addr1 u1 is the rest
    0 $split 2swap ;

: .path ( path-addr -- ) \ gforth
    \G Display the contents of the search path @var{path-addr}.
    path>string
    BEGIN next-path dup WHILE type space REPEAT 2drop 2drop ;

: .fpath ( -- ) \ gforth
    \G Display the contents of the Forth search path.
    fpath .path ;

: absolute-file? ( addr u -- flag ) \ gforth
    \G A filename is absolute if it starts with a / or a ~ (~ expansion),
    \G or if it is in the form ./*, extended regexp: ^[/~]|./, or if
    \G it has a colon as second character ("C:...").  Paths simply
    \G containing a / are not absolute!
    2dup 2 u> swap 1+ c@ ':' = and >r \ dos absoulte: c:/....
    over c@ '/' = >r
    over c@ '~' = >r
    \ 2dup S" ../" string-prefix? r> or >r \ not catered for in expandtopic
    S" ./" string-prefix?
    r> r> r> or or or ;

: pathsep? dup '/' = swap '\' = or ;

: need/   ofile $@ 1- + c@ pathsep? 0= IF s" /" ofile $+! THEN ;

: extractpath ( adr len -- adr len2 )
  BEGIN dup WHILE 1-
        2dup + c@ pathsep? IF EXIT THEN
  REPEAT ;

: remove~+ ( -- )
    ofile $@ s" ~+/" string-prefix?
    IF
	ofile 0 3 $del
    THEN ;

: expandtopic ( -- )
    \ stack effect correct? - anton
    \ expands "./" into an absolute name
    ofile $@ s" ./" string-prefix?
    IF
	ofile $@ 1 /string tfile $!
	sourcefilename extractpath ofile $!
	\ care of / only if there is a directory
	ofile $@len IF need/ THEN
	tfile $@ over c@ pathsep? IF 1 /string THEN
	ofile $+!
    THEN ;

: del-string ( addr u u1 -- addr u2 )
    \ delete u1 characters from string by moving stuff from further up
    third >r safe/string r@ over >r swap move 2r> ;

: del-./s ( addr u -- addr u2 )
    \ deletes (/*./)* at the start of the string
    BEGIN ( current-addr u )
	BEGIN ( current-addr u )
	    dup WHILE
	    over c@ '/' = WHILE
		1 del-string
	REPEAT  THEN
	2dup s" ./" string-prefix? WHILE
	    2 del-string
    REPEAT ;

: preserve-root ( addr1 u1 -- addr2 u2 )
    dup if
	over c@ '/' = if \ preserve / at start
	    1 safe/string
	then
    then ;

: skip-..-prefixes ( addr1 u1 -- addr2 u2 )
    \ deal with ../ at start
    begin ( current-addr u )
	del-./s 2dup s" ../" string-prefix? while
	    3 /string
    repeat ;
    
: compact-filename ( addr u1 -- addr u2 )
    \ rewrite filename in place, eliminating multiple slashes, "./", and "x/.."
    over swap preserve-root skip-..-prefixes
    ( start current-addr u )
    over swap '/' scan dup if ( start addr3 addr4 u4 )
	1 safe/string del-./s compact-filename
	2dup s" ../" string-prefix? if ( start addr3 addr4 u4 )
	    3 safe/string ( start to from count )
	    >r swap 2dup r@ move r>
	endif
    endif
    + nip over - ;

\ test cases:
\ s" z/../../../a" compact-filename type cr
\ s" ../z/../../../a/c" compact-filename type cr
\ s" /././//./../..///x/y/../z/.././..//..//a//b/../c" compact-filename type cr

: reworkdir ( -- )
  remove~+
  ofile $@ compact-filename
  nip ofile $!len ;

: open-ofile ( -- wfileid ior )
    \G opens the file whose name is in ofile
    ofile $@ r/o open-file ;

: execute-path-file ( addr1 u1 xt path-addr -- wfileid addr2 u2 0 | ior ) \ gforth-internal
    2>r
    2dup absolute-file? IF
        rdrop
	ofile $! expandtopic reworkdir r> execute
        dup 0= IF
            >r ofile $@ r> THEN
        EXIT
    ELSE
        r> -&37 >r path>string BEGIN
            next-path dup WHILE
		rdrop ofile $! need/ 2over ofile $+! expandtopic reworkdir
		r@ execute dup 0= IF
                    drop >r 2drop 2drop r> ofile $@ 0  rdrop EXIT
                ELSE
                    >r drop
                THEN
        REPEAT
        2drop 2drop 2drop r>
  THEN  rdrop ;

: open-path-file ( addr1 u1 path-addr -- wfileid addr2 u2 0 | ior ) \ gforth
\G Look in path @var{path-addr} for the file specified by @var{addr1
\G u1}.  If found, the resulting path and an (read-only) open file
\G descriptor are returned. If the file is not found, @var{ior} is
\G what came back from the last attempt at opening the file (in the
    \G current implementation).
    ['] open-ofile swap execute-path-file ;

: open-fpath-file ( addr1 u1 -- wfileid addr2 u2 0 | ior ) \ gforth-internal
    \G Look in the Forth search path for the file specified by @var{addr1 u1}.
    \G If found, the resulting path and an open file descriptor
    \G are returned. If the file is not found, @var{ior} is non-zero.
    fpath open-path-file ;
