
// Bindings for PlutoSVG (plutosvg.h translated to C#).
// See https://github.com/sammycage/plutosvg for the original source
// and license information.

/*
 * Copyright (c) 2020-2024 Samuel Ugochukwu <sammycageagle@gmail.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Runtime.InteropServices;

using plutovg_destroy_func_t = PlutoVG.PlutoVG.plutovg_destroy_func_t;
using plutovg_color_t = PlutoVG.PlutoVG.plutovg_color_t;
using plutovg_canvas_t = PlutoVG.PlutoVG.plutovg_canvas_t;
using plutovg_surface_t = PlutoVG.PlutoVG.plutovg_surface_t;
using plutovg_rect_t = PlutoVG.PlutoVG.plutovg_rect_t;

namespace PlutoSVG;


public static class PlutoSVG
{
    private const string nativeLibName = "plutosvg";

    /**
     * @brief Returns the version number of PlutoSVG.
     *
     * @return The version number as an integer.
     */
    [DllImport(nativeLibName, EntryPoint= "plutosvg_version", CallingConvention = CallingConvention.Cdecl)]
    private static extern int inner_plutosvg_version();

    public static (int Major, int Minor, int Micro) plutosvg_version()
    {
        int EncodedVersion = inner_plutosvg_version();
        int MajorVersion = EncodedVersion / 10000;
        int MinorVersion = (EncodedVersion % 10000) / 100;
        int MicroVersion = (EncodedVersion % 100);
        return (MajorVersion, MinorVersion, MicroVersion);
    }

    public static string plutosvg_version_string()
    {
        (int Major, int Minor, int Micro) = plutosvg_version();
        return $"{Major}.{Minor}.{Micro}";
    }

    /**
     * @brief plutosvg_document_t
     */
    public struct plutosvg_document_t
    {

    }

    /**
     * @brief Callback type for resolving CSS color variables in SVG documents.
     *
     * @param closure User-defined data for the callback.
     * @param name Name of the color variable.
     * @param length Length of the color variable name.
     * @param color Pointer to `plutovg_color_t` where the resolved color will be stored.
     * @return `true` if the color variable was successfully resolved; `false` otherwise.
     */
    //typedef bool(*plutosvg_palette_func_t)(void* closure, const char* name, int length, plutovg_color_t* color);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate bool plutosvg_palette_func_t(void* closure, /*const*/ byte* name, int length, plutovg_color_t* color);

    /**
     * @brief Loads an SVG document from a data buffer.
     *
     * @note The buffer pointed to by `data` must remain valid until the `plutosvg_document_t` object is destroyed.
     *
     * @param data Pointer to the SVG data buffer.
     * @param length Length of the data buffer.
     * @param width Container width for resolving the initial viewport.
     * @param height Container height for resolving the initial viewport.
     * @param destroy_func Custom function to call when the document is destroyed.
     * @param closure User-defined data for the `destroy_func` callback.
     * @return Pointer to the loaded `plutosvg_document_t` structure, or NULL if loading fails.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutosvg_document_t* plutosvg_document_load_from_data(/*const*/ void* data, int length, float width, float height,
                                                                       plutovg_destroy_func_t destroy_func, void* closure);

    /**
     * @brief Loads an SVG document from a file.
     *
     * @param filename Path to the SVG file.
     * @param width Container width for resolving the initial viewport.
     * @param height Container height for resolving the initial viewport.
     * @return Pointer to the loaded `plutosvg_document_t` structure, or NULL if loading fails.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutosvg_document_t* plutosvg_document_load_from_file(/*const*/ char* filename, float width, float height);

    /**
     * @brief Renders an SVG document or a specific element onto a canvas.
     *
     * @param document Pointer to the SVG document.
     * @param id ID of the SVG element to render, or `NULL` to render the entire document.
     * @param canvas Canvas onto which the SVG element or document will be rendered.
     * @param current_color Color used to resolve CSS `currentColor` values.
     * @param palette_func Callback for resolving CSS color variables.
     * @param closure User-defined data for the `palette_func` callback.
     * @return `true` if rendering was successful; `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutosvg_document_render(/*const*/ plutosvg_document_t* document, /*const*/ char* id, plutovg_canvas_t* canvas,
                                               /*const*/ plutovg_color_t* current_color, plutosvg_palette_func_t palette_func, void* closure);

    /**
     * @brief Renders an SVG document or a specific element onto a surface.
     *
     * @param document Pointer to the SVG document.
     * @param id ID of the SVG element to render, or `NULL` to render the entire document.
     * @param width Width of the surface, or `-1` if unspecified.
     * @param height Height of the surface, or `-1` if unspecified.
     * @param current_color Color for resolving CSS `currentColor` values.
     * @param palette_func Callback for resolving CSS color variables.
     * @param closure User-defined data for the `palette_func` callback.
     * @return Pointer to the rendered `plutovg_surface_t` structure, or `NULL` if rendering fails.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutosvg_document_render_to_surface(/*const*/ plutosvg_document_t* document, /*const*/ char* id, int width, int height,
                                                                        /*const*/ plutovg_color_t* current_color, plutosvg_palette_func_t palette_func, void* closure);

    /**
     * @brief Returns the intrinsic width of the SVG document.
     *
     * @param document Pointer to the SVG document.
     * @return The intrinsic width of the SVG document.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutosvg_document_get_width(/*const*/ plutosvg_document_t* document);

    /**
     * @brief Returns the intrinsic height of the SVG document.
     *
     * @param document Pointer to the SVG document.
     * @return The intrinsic height of the SVG document.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutosvg_document_get_height(/*const*/ plutosvg_document_t* document);

    /**
     * @brief Retrieves the bounding box of a specific element or the entire SVG document.
     *
     * Calculates and retrieves the extents of an element identified by `id` or the whole document if `id` is `NULL`.
     *
     * @param document Pointer to the SVG document.
     * @param id ID of the element whose extents to retrieve, or `NULL` to retrieve the extents of the entire document.
     * @param extents Pointer to a `plutovg_rect_t` structure where the extents will be stored.
     * @return `true` if extents were successfully retrieved; `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutosvg_document_extents(/*const*/ plutosvg_document_t* document, /*const*/ char* id, plutovg_rect_t* extents);

    /**
     * @brief Destroys an SVG document and frees its resources.
     *
     * @param document Pointer to a `plutosvg_document_t` structure to be destroyed. If `NULL`, the function does nothing.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutosvg_document_destroy(plutosvg_document_t* document);

    /**
     * @brief Retrieves PlutoSVG hooks for integrating with FreeType's SVG module.
     *
     * Provides hooks that allow FreeType to use PlutoSVG for rendering SVG graphics in fonts.
     *
     * @return Pointer to the structure containing PlutoSVG hooks for FreeType's SVG module, or `NULL` if FreeType integration is not enabled.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe /*const*/ void* plutosvg_ft_svg_hooks();

}
