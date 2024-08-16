\ display words in reverse with pattern matching

\ Author: Bernd Paysan
\ Copyright (C) 2021,2023 Free Software Foundation, Inc.

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

\ solution for wordlists greater than the stack
Variable words[]

Defer mword-match
: mword-search-match ( addr1 u1 addr2 u2 -- flag )
    capssearch nip nip ;
: mword-filename-match ( addr1 u1 addr2 u2 -- flag )
    dup 0= IF  2drop s" *"  THEN  filename-match ;
' mword-search-match is mword-match

: wid>words[] ( wid -- )
    [: words[] >stack true ;] swap traverse-wordlist ;
: .mwords[] ( addr u -- ) 0 { n }
    cr words[] $@ cell MEM-DO
	I @ name>string 2over mword-match
	IF  n I @ .word to n  THEN
    LOOP  2drop ;

: wordlist-mwords ( addr u wid -- )  wid>words[] .mwords[] words[] $free ;
: mwords ( ["pattern"] -- ) \ gforth
    \G list all words matching the optional parameter @var{pattern};
    \G if none, all words match.  Words are listed old to new.
    \G Pattern match like @code{search} (default), you can switch to
    \G globbing with @code{' mword-filename-match is mword-match}.
    parse-name context @ wordlist-mwords ;
