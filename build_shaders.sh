#!/usr/bin/env sh
glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S vert -o draw_splat.vs.spirv star_machine/draw_splat.vs.glsl
glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S frag -o draw_splat.fs.spirv star_machine/draw_splat.fs.glsl

glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S vert -o DrawOverlay.vs.spirv star_machine/DrawOverlay.vs.glsl
glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S frag -o DrawOverlay.fs.spirv star_machine/DrawOverlay.fs.glsl

glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S vert -o Reveal.vs.spirv star_machine/Reveal.vs.glsl
glslangValidator --enhanced-msgs --nan-clamp -g -V -e main -S frag -o Reveal.fs.spirv star_machine/Reveal.fs.glsl
