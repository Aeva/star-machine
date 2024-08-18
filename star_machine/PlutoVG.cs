
// Bindings for PlutoVG (plutovg.h translated to C#).
// See https://github.com/sammycage/plutovg for the original source
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
using System.Runtime.CompilerServices;
using plutovg_codepoint_t = int;

namespace PlutoVG;


public static class PlutoVG
{
    private const string nativeLibName = "plutovg-0";

    /**
     * @brief Gets the version of the plutovg library.
     * @return An integer representing the version of the plutovg library.
     */
    [DllImport(nativeLibName, EntryPoint= "plutovg_version", CallingConvention = CallingConvention.Cdecl)]
    private static extern int inner_plutovg_version();

    public static (int Major, int Minor, int Micro) plutovg_version()
    {
        int EncodedVersion = inner_plutovg_version();
        int MajorVersion = EncodedVersion / 10000;
        int MinorVersion = (EncodedVersion % 10000) / 100;
        int MicroVersion = (EncodedVersion % 100);
        return (MajorVersion, MinorVersion, MicroVersion);
    }

    public static string plutovg_version_string()
    {
        (int Major, int Minor, int Micro) = plutovg_version();
        return $"{Major}.{Minor}.{Micro}";
    }

    /**
     * @brief A function pointer type for a cleanup callback.
     * @param closure A pointer to the resource to be cleaned up.
     */
    //typedef void (*plutovg_destroy_func_t)(void* closure);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void plutovg_destroy_func_t(IntPtr closure);

    /**
     * @brief A function pointer type for a write callback.
     * @param closure A pointer to user-defined data or context.
     * @param data A pointer to the data to be written.
     * @param size The size of the data in bytes.
     */
    //typedef void (*plutovg_write_func_t)(void* closure, void* data, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void plutovg_write_func_t(void* closure, void* data, int size);

    /**
     * @brief A structure representing a point in 2D space.
     */
    public struct plutovg_point_t {
        public float x; ///< The x-coordinate of the point.
        public float y; ///< The y-coordinate of the point.
    }

    /**
     * @brief A structure representing a rectangle in 2D space.
     */
    public struct plutovg_rect_t {
        public float x; ///< The x-coordinate of the top-left corner of the rectangle.
        public float y; ///< The y-coordinate of the top-left corner of the rectangle.
        public float w; ///< The width of the rectangle.
        public float h; ///< The height of the rectangle.
    }

    /**
     * @brief A structure representing a 2D transformation matrix.
     */
    public struct plutovg_matrix_t {
        public float a; ///< The horizontal scaling factor.
        public float b; ///< The vertical shearing factor.
        public float c; ///< The horizontal shearing factor.
        public float d; ///< The vertical scaling factor.
        public float e; ///< The horizontal translation offset.
        public float f; ///< The vertical translation offset.
    }

    /**
     * @brief Initializes a 2D transformation matrix.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     * @param a The horizontal scaling factor.
     * @param b The vertical shearing factor.
     * @param c The horizontal shearing factor.
     * @param d The vertical scaling factor.
     * @param e The horizontal translation offset.
     * @param f The vertical translation offset.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init(plutovg_matrix_t* matrix, float a, float b, float c, float d, float e, float f);

    /**
     * @brief Initializes a 2D transformation matrix to the identity matrix.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init_identity(plutovg_matrix_t* matrix);

    /**
     * @brief Initializes a 2D transformation matrix for translation.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     * @param tx The translation offset in the x-direction.
     * @param ty The translation offset in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init_translate(plutovg_matrix_t* matrix, float tx, float ty);

    /**
     * @brief Initializes a 2D transformation matrix for scaling.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     * @param sx The scaling factor in the x-direction.
     * @param sy The scaling factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init_scale(plutovg_matrix_t* matrix, float sx, float sy);

    /**
     * @brief Initializes a 2D transformation matrix for shearing.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     * @param shx The shearing factor in the x-direction.
     * @param shy The shearing factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init_shear(plutovg_matrix_t* matrix, float shx, float shy);

    /**
     * @brief Initializes a 2D transformation matrix for rotation.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be initialized.
     * @param angle The rotation angle in radians.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_init_rotate(plutovg_matrix_t* matrix, float angle);

    /**
     * @brief Adds a translation with offsets `tx` and `ty` to the matrix.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be modified.
     * @param tx The translation offset in the x-direction.
     * @param ty The translation offset in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_translate(plutovg_matrix_t* matrix, float tx, float ty);

    /**
     * @brief Scales the matrix by factors `sx` and `sy`
     * @param matrix A pointer to the `plutovg_matrix_t` object to be modified.
     * @param sx The scaling factor in the x-direction.
     * @param sy The scaling factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_scale(plutovg_matrix_t* matrix, float sx, float sy);

    /**
     * @brief Shears the matrix by factors `shx` and `shy`.
     * @param matrix A pointer to the `plutovg_matrix_t` object to be modified.
     * @param shx The shearing factor in the x-direction.
     * @param shy The shearing factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_shear(plutovg_matrix_t* matrix, float shx, float shy);

    /**
     * @brief Rotates the matrix by the specified angle (in radians).
     * @param matrix A pointer to the `plutovg_matrix_t` object to be modified.
     * @param angle The rotation angle in radians.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_rotate(plutovg_matrix_t* matrix, float angle);

    /**
     * @brief Multiplies `left` and `right` matrices and stores the result in `matrix`.
     * @note `matrix` can be identical to either `left` or `right`.
     * @param matrix A pointer to the `plutovg_matrix_t` object to store the result.
     * @param left A pointer to the first `plutovg_matrix_t` matrix.
     * @param right A pointer to the second `plutovg_matrix_t` matrix.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_multiply(plutovg_matrix_t* matrix, /*const*/ plutovg_matrix_t* left, /*const*/ plutovg_matrix_t* right);

    /**
     * @brief Calculates the inverse of `matrix` and stores it in `inverse`.
     *
     * If `inverse` is `NULL`, the function only checks if the matrix is invertible.
     *
     * @note `matrix` and `inverse` can be identical.
     * @param matrix A pointer to the `plutovg_matrix_t` object to invert.
     * @param inverse A pointer to the `plutovg_matrix_t` object to store the result, or `NULL`.
     * @return `true` if the matrix is invertible; `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_matrix_invert(/*const*/ plutovg_matrix_t* matrix, plutovg_matrix_t* inverse);

    /**
     * @brief Transforms the point `(x, y)` using `matrix` and stores the result in `(xx, yy)`.
     * @param matrix A pointer to a `plutovg_matrix_t` object.
     * @param x The x-coordinate of the point to transform.
     * @param y The y-coordinate of the point to transform.
     * @param xx A pointer to store the transformed x-coordinate.
     * @param yy A pointer to store the transformed y-coordinate.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_map(/*const*/ plutovg_matrix_t* matrix, float x, float y, float* xx, float* yy);

    /**
     * @brief Transforms the `src` point using `matrix` and stores the result in `dst`.
     * @note `src` and `dst` can be identical.
     * @param matrix A pointer to a `plutovg_matrix_t` object.
     * @param src A pointer to the `plutovg_point_t` object to transform.
     * @param dst A pointer to the `plutovg_point_t` to store the transformed point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_map_point(/*const*/ plutovg_matrix_t* matrix, /*const*/ plutovg_point_t* src, plutovg_point_t* dst);

    /**
     * @brief Transforms an array of `src` points using `matrix` and stores the results in `dst`.
     * @note `src` and `dst` can be identical.
     * @param matrix A pointer to a `plutovg_matrix_t` object.
     * @param src A pointer to the array of `plutovg_point_t` objects to transform.
     * @param dst A pointer to the array of `plutovg_point_t` to store the transformed points.
     * @param count The number of points to transform.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_map_points(/*const*/ plutovg_matrix_t* matrix, /*const*/ plutovg_point_t* src, plutovg_point_t* dst, int count);

    /**
     * @brief Transforms the `src` rectangle using `matrix` and stores the result in `dst`.
     * @note `src` and `dst` can be identical.
     * @param matrix A pointer to a `plutovg_matrix_t` object.
     * @param src A pointer to the `plutovg_rect_t` object to transform.
     * @param dst A pointer to the `plutovg_rect_t` to store the transformed rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_matrix_map_rect(/*const*/ plutovg_matrix_t* matrix, /*const*/ plutovg_rect_t* src, plutovg_rect_t* dst);

    /**
     * @brief Represents a 2D path for drawing operations.
     */
    public struct plutovg_path_t
    {
    };

    /**
     * @brief Enumeration defining path commands.
     */
    public enum plutovg_path_command_t : int {
        PLUTOVG_PATH_COMMAND_MOVE_TO, ///< Moves the current point to a new position.
        PLUTOVG_PATH_COMMAND_LINE_TO, ///< Draws a straight line to a new point.
        PLUTOVG_PATH_COMMAND_CUBIC_TO, ///< Draws a cubic Bézier curve to a new point.
        PLUTOVG_PATH_COMMAND_CLOSE ///< Closes the current path by drawing a line to the starting point.
    }

    /**
     * @brief Union representing a path element.
     *
     * A path element can be a command with a length or a coordinate point.
     * Each command type in the path element array is followed by a specific number of points:
     * - `PLUTOVG_PATH_COMMAND_MOVE_TO`: 1 point
     * - `PLUTOVG_PATH_COMMAND_LINE_TO`: 1 point
     * - `PLUTOVG_PATH_COMMAND_CUBIC_TO`: 3 points
     * - `PLUTOVG_PATH_COMMAND_CLOSE`: 1 point
     *
     * @example
     * const plutovg_path_element_t* elements;
     * int count = plutovg_path_get_elements(path, &elements);
     * for(int i = 0; i < count; i += elements[i].header.length) {
     *     plutovg_path_command_t command = elements[i].header.command;
     *     switch(command) {
     *     case PLUTOVG_PATH_COMMAND_MOVE_TO:
     *         printf("MoveTo: %g %g\n", elements[i + 1].point.x, elements[i + 1].point.y);
     *         break;
     *     case PLUTOVG_PATH_COMMAND_LINE_TO:
     *         printf("LineTo: %g %g\n", elements[i + 1].point.x, elements[i + 1].point.y);
     *         break;
     *     case PLUTOVG_PATH_COMMAND_CUBIC_TO:
     *         printf("CubicTo: %g %g %g %g %g %g\n",
     *                elements[i + 1].point.x, elements[i + 1].point.y,
     *                elements[i + 2].point.x, elements[i + 2].point.y,
     *                elements[i + 3].point.x, elements[i + 3].point.y);
     *         break;
     *     case PLUTOVG_PATH_COMMAND_CLOSE:
     *         printf("Close: %g %g\n", elements[i + 1].point.x, elements[i + 1].point.y);
     *         break;
     *     }
     * }
     */
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct plutovg_path_element_t
    {
        public struct header_t
        {
#pragma warning disable CS0169 // disable "never used" warnings
            plutovg_path_command_t command; ///< The path command.
            int length; ///< Number of elements including the header.
#pragma warning restore CS0169
        }

        [System.Runtime.InteropServices.FieldOffset(0)]
        public header_t header; ///< Header for path commands.

        [System.Runtime.InteropServices.FieldOffset(0)]
        public plutovg_point_t point; ///< A coordinate point in the path.

        public plutovg_path_element_t(header_t in_header)
        {
            Unsafe.SkipInit(out point);
            header = in_header;
        }

        public plutovg_path_element_t(plutovg_point_t in_point)
        {
            Unsafe.SkipInit(out header);
            point = in_point;
        }
    }


    /**
     * @brief Iterator for traversing path elements in a path.
     */
    public struct plutovg_path_iterator_t {
        public unsafe /*const*/ plutovg_path_element_t* elements; ///< Pointer to the array of path elements.
        public int size; ///< Total number of elements in the array.
        public int index; ///< Current position in the array.
    };


    /**
     * @brief Initializes a path iterator for a given path.
     *
     * @param it The path iterator to initialize.
     * @param path The path to iterate over.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_iterator_init(plutovg_path_iterator_t* it, /*const*/ plutovg_path_t* path);

    /**
     * @brief Checks if there are more elements to iterate over.
     *
     * @param it The path iterator.
     * @return `true` if there are more elements; otherwise, `false`.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_path_iterator_has_next(/*const*/ plutovg_path_iterator_t* it);

    /**
     * @brief Retrieves the current command and its associated points, then advances the iterator.
     *
     * @param it The path iterator.
     * @param points An array to store the points for the current command.
     * @return The path command for the current element.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_command_t plutovg_path_iterator_next(plutovg_path_iterator_t* it, plutovg_point_t[/*3*/] point);

    /**
     * @brief Creates a new path object.
     *
     * @return A pointer to the newly created path object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_path_create();

    /**
     * @brief Increases the reference count of a path object.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @return A pointer to the same `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_path_reference(plutovg_path_t* path);

    /**
     * @brief Decreases the reference count of a path object.
     *
     * This function decrements the reference count of the given path object. If
     * the reference count reaches zero, the path object is destroyed and its
     * resources are freed.
     *
     * @param path A pointer to the `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_destroy(plutovg_path_t* path);

    /**
     * @brief Retrieves the reference count of a path object.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @return The current reference count of the path object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_path_get_reference_count(/*const*/ plutovg_path_t* path);

    /**
     * @brief Retrieves the elements of a path.
     *
     * Provides access to the array of path elements.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param elements A pointer to a pointer that will be set to the array of path elements.
     * @return The number of elements in the path.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_path_get_elements(/*const*/ plutovg_path_t* path, /*const*/ plutovg_path_element_t** elements);

    /**
     * @brief Moves the current point to a new position.
     *
     * This function moves the current point to the specified coordinates without
     * drawing a line. This is equivalent to the `M` command in SVG path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x The x-coordinate of the new position.
     * @param y The y-coordinate of the new position.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_move_to(plutovg_path_t* path, float x, float y);

    /**
     * @brief Adds a straight line segment to the path.
     *
     * This function adds a straight line segment from the current point to the
     * specified coordinates. This is equivalent to the `L` command in SVG path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x The x-coordinate of the end point of the line segment.
     * @param y The y-coordinate of the end point of the line segment.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_line_to(plutovg_path_t* path, float x, float y);

    /**
     * @brief Adds a quadratic Bézier curve to the path.
     *
     * This function adds a quadratic Bézier curve segment from the current point
     * to the specified end point, using the given control point. This is equivalent
     * to the `Q` command in SVG path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x1 The x-coordinate of the control point.
     * @param y1 The y-coordinate of the control point.
     * @param x2 The x-coordinate of the end point of the curve.
     * @param y2 The y-coordinate of the end point of the curve.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_quad_to(plutovg_path_t* path, float x1, float y1, float x2, float y2);

    /**
     * @brief Adds a cubic Bézier curve to the path.
     *
     * This function adds a cubic Bézier curve segment from the current point
     * to the specified end point, using the given two control points. This is
     * equivalent to the `C` command in SVG path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x1 The x-coordinate of the first control point.
     * @param y1 The y-coordinate of the first control point.
     * @param x2 The x-coordinate of the second control point.
     * @param y2 The y-coordinate of the second control point.
     * @param x3 The x-coordinate of the end point of the curve.
     * @param y3 The y-coordinate of the end point of the curve.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_cubic_to(plutovg_path_t* path, float x1, float y1, float x2, float y2, float x3, float y3);

    /**
     * @brief Adds an elliptical arc to the path.
     *
     * This function adds an elliptical arc segment from the current point to the
     * specified end point. The arc is defined by the radii, rotation angle, and
     * flags for large arc and sweep. This is equivalent to the `A` command in SVG
     * path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param rx The x-radius of the ellipse.
     * @param ry The y-radius of the ellipse.
     * @param angle The rotation angle of the ellipse in radians.
     * @param large_arc_flag If true, draw the large arc; otherwise, draw the small arc.
     * @param sweep_flag If true, draw the arc in the positive-angle direction; otherwise, in the negative-angle direction.
     * @param x The x-coordinate of the end point of the arc.
     * @param y The y-coordinate of the end point of the arc.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_arc_to(plutovg_path_t* path, float rx, float ry, float angle, bool large_arc_flag, bool sweep_flag, float x, float y);

    /**
     * @brief Closes the current sub-path.
     *
     * This function closes the current sub-path by drawing a straight line back to
     * the start point of the sub-path. This is equivalent to the `Z` command in SVG
     * path syntax.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_close(plutovg_path_t* path);

    /**
     * @brief Retrieves the current point of the path.
     *
     * Gets the current point's coordinates in the path. This point is the last
     * position used or the point where the path was last moved to.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x The x-coordinate of the current point.
     * @param y The y-coordinate of the current point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_get_current_point(/*const*/ plutovg_path_t* path, float* x, float* y);

    /**
     * @brief Reserves space for path elements.
     *
     * Reserves space for a specified number of elements in the path. This helps optimize
     * memory allocation for future path operations.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param count The number of path elements to reserve space for.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_reserve(plutovg_path_t* path, int count);

    /**
     * @brief Resets the path.
     *
     * Clears all path data, effectively resetting the `plutovg_path_t` object to its initial state.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_reset(plutovg_path_t* path);

    /**
     * @brief Adds a rectangle to the path.
     *
     * Adds a rectangle defined by the top-left corner (x, y) and dimensions (w, h) to the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x The x-coordinate of the rectangle's top-left corner.
     * @param y The y-coordinate of the rectangle's top-left corner.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_rect(plutovg_path_t* path, float x, float y, float w, float h);

    /**
     * @brief Adds a rounded rectangle to the path.
     *
     * Adds a rounded rectangle defined by the top-left corner (x, y), dimensions (w, h),
     * and corner radii (rx, ry) to the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param x The x-coordinate of the rectangle's top-left corner.
     * @param y The y-coordinate of the rectangle's top-left corner.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     * @param rx The x-radius of the rectangle's corners.
     * @param ry The y-radius of the rectangle's corners.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_round_rect(plutovg_path_t* path, float x, float y, float w, float h, float rx, float ry);

    /**
     * @brief Adds an ellipse to the path.
     *
     * Adds an ellipse defined by the center (cx, cy) and radii (rx, ry) to the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param cx The x-coordinate of the ellipse's center.
     * @param cy The y-coordinate of the ellipse's center.
     * @param rx The x-radius of the ellipse.
     * @param ry The y-radius of the ellipse.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_ellipse(plutovg_path_t* path, float cx, float cy, float rx, float ry);

    /**
     * @brief Adds a circle to the path.
     *
     * Adds a circle defined by its center (cx, cy) and radius (r) to the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param cx The x-coordinate of the circle's center.
     * @param cy The y-coordinate of the circle's center.
     * @param r The radius of the circle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_circle(plutovg_path_t* path, float cx, float cy, float r);

    /**
     * @brief Adds an arc to the path.
     *
     * Adds an arc defined by the center (cx, cy), radius (r), start angle (a0), end angle (a1),
     * and direction (ccw) to the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param cx The x-coordinate of the arc's center.
     * @param cy The y-coordinate of the arc's center.
     * @param r The radius of the arc.
     * @param a0 The start angle of the arc in radians.
     * @param a1 The end angle of the arc in radians.
     * @param ccw If true, the arc is drawn counter-clockwise; if false, clockwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_arc(plutovg_path_t* path, float cx, float cy, float r, float a0, float a1, bool ccw);

    /**
     * @brief Adds a sub-path to the path.
     *
     * Adds all elements from another path (`source`) to the current path, optionally
     * applying a transformation matrix.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param source A pointer to the `plutovg_path_t` object to copy elements from.
     * @param matrix A pointer to a `plutovg_matrix_t` object, or `NULL` to apply no transformation.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_add_path(plutovg_path_t* path, /*const*/ plutovg_path_t* source, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Applies a transformation matrix to the path.
     *
     * Transforms the entire path using the provided transformation matrix.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param matrix A pointer to a `plutovg_matrix_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_transform(plutovg_path_t* path, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Callback function type for traversing a path.
     *
     * This function type defines a callback used to traverse path elements.
     *
     * @param closure A pointer to user-defined data passed to the callback.
     * @param command The current path command.
     * @param points An array of points associated with the command.
     * @param npoints The number of points in the array.
     */
    //typedef void (*plutovg_path_traverse_func_t)(void* closure, plutovg_path_command_t command, const plutovg_point_t* points, int npoints);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void plutovg_path_traverse_func_t(void* closure, plutovg_path_command_t command, /*const*/ plutovg_point_t* points, int npoints);

    /**
     * @brief Traverses the path and calls the callback for each element.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param traverse_func The callback function to be called for each element of the path.
     * @param closure User-defined data passed to the callback.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_traverse(/*const*/ plutovg_path_t* path, plutovg_path_traverse_func_t traverse_func, void* closure);

    /**
     * @brief Traverses the path with Bézier curves flattened to line segments.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param traverse_func The callback function to be called for each element of the path.
     * @param closure User-defined data passed to the callback.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_traverse_flatten(/*const*/ plutovg_path_t* path, plutovg_path_traverse_func_t traverse_func, void* closure);

    /**
     * @brief Traverses the path with a dashed pattern and calls the callback for each segment.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param offset The starting offset into the dash pattern.
     * @param dashes An array of dash lengths.
     * @param ndashes The number of elements in the `dashes` array.
     * @param traverse_func The callback function to be called for each element of the path.
     * @param closure User-defined data passed to the callback.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_path_traverse_dashed(/*const*/ plutovg_path_t* path, float offset, /*const*/ float* dashes, int ndashes, plutovg_path_traverse_func_t traverse_func, void* closure);

    /**
     * @brief Creates a copy of the path.
     *
     * @param path A pointer to the `plutovg_path_t` object to clone.
     * @return A pointer to the newly created path clone.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_path_clone(/*const*/ plutovg_path_t* path);

    /**
     * @brief Creates a copy of the path with Bézier curves flattened to line segments.
     *
     * @param path A pointer to the `plutovg_path_t` object to clone.
     * @return A pointer to the newly created path clone with flattened curves.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_path_clone_flatten(/*const*/ plutovg_path_t* path);

    /**
     * @brief Creates a copy of the path with a dashed pattern applied.
     *
     * @param path A pointer to the `plutovg_path_t` object to clone.
     * @param offset The starting offset into the dash pattern.
     * @param dashes An array of dash lengths.
     * @param ndashes The number of elements in the `dashes` array.
     * @return A pointer to the newly created path clone with dashed pattern.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_path_clone_dashed(/*const*/ plutovg_path_t* path, float offset, /*const*/ float* dashes, int ndashes);

    /**
     * @brief Computes the bounding box and total length of the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @param extents A pointer to a `plutovg_rect_t` object to store the bounding box.
     * @return The total length of the path.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_path_extents(/*const*/ plutovg_path_t* path, plutovg_rect_t* extents);

    /**
     * @brief Calculates the total length of the path.
     *
     * @param path A pointer to a `plutovg_path_t` object.
     * @return The total length of the path.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_path_length(/*const*/ plutovg_path_t* path);

    /**
     * @brief Parses SVG path data into a `plutovg_path_t` object.
     *
     * @param path A pointer to the `plutovg_path_t` object to populate.
     * @param data The SVG path data string.
     * @param length The length of `data`, or `-1` for null-terminated data.
     * @return `true` if successful; `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_path_parse(plutovg_path_t* path, /*const*/ char* data, int length);

    /**
     * @brief Text encodings used for converting text data to code points.
     */
    public enum plutovg_text_encoding_t : int {
        PLUTOVG_TEXT_ENCODING_UTF8, ///< UTF-8 encoding
        PLUTOVG_TEXT_ENCODING_UTF16, ///< UTF-16 encoding
        PLUTOVG_TEXT_ENCODING_UTF32, ///< UTF-32 encoding
        PLUTOVG_TEXT_ENCODING_LATIN1 ///< Latin-1 encoding
    }

    /**
     * @brief Iterator for traversing code points in text data.
     */
    public struct plutovg_text_iterator_t {
        public unsafe /*const*/ void* text; ///< Pointer to the text data.
        public int length; ///< Length of the text data.
        public plutovg_text_encoding_t encoding; ///< Encoding format of the text data.
        public int index; ///< Current position in the text data.
    }

#if false
    /**
     * @brief Represents a Unicode code point.
     */
    typedef unsigned int plutovg_codepoint_t;
#endif

    /**
     * @brief Initializes a text iterator.
     *
     * @param it Pointer to the text iterator.
     * @param text Pointer to the text data.
     * @param length Length of the text data, or -1 if the data is null-terminated.
     * @param encoding Encoding of the text data.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_text_iterator_init(plutovg_text_iterator_t* it, /*const*/ void* text, int length, plutovg_text_encoding_t encoding);

    /**
     * @brief Checks if there are more code points to iterate.
     *
     * @param it Pointer to the text iterator.
     * @return `true` if more code points are available; otherwise, `false`.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_text_iterator_has_next(/*const*/ plutovg_text_iterator_t* it);

    /**
     * @brief Retrieves the next code point and advances the iterator.
     *
     * @param it Pointer to the text iterator.
     * @return The next code point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_codepoint_t plutovg_text_iterator_next(plutovg_text_iterator_t* it);

    /**
     * @brief Represents a font face.
     */
    public struct plutovg_font_face_t
    {

    };

    /**
     * @brief Loads a font face from a file.
     *
     * @param filename Path to the font file.
     * @param ttcindex Index of the font face within a TrueType Collection (TTC).
     * @return A pointer to the loaded `plutovg_font_face_t` object, or `NULL` on failure.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_font_face_t* plutovg_font_face_load_from_file(/*const*/ char* filename, int ttcindex);

    /**
     * @brief Loads a font face from memory.
     *
     * @param data Pointer to the font data.
     * @param length Length of the font data.
     * @param ttcindex Index of the font face within a TrueType Collection (TTC).
     * @param destroy_func Function to free the font data when no longer needed.
     * @param closure User-defined data passed to `destroy_func`.
     * @return A pointer to the loaded `plutovg_font_face_t` object, or `NULL` on failure.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_font_face_t* plutovg_font_face_load_from_data(/*const*/ void* data, uint length, int ttcindex, plutovg_destroy_func_t destroy_func, void* closure);

    /**
     * @brief Increments the reference count of a font face.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @return A pointer to the same `plutovg_font_face_t` object with an incremented reference count.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_font_face_t* plutovg_font_face_reference(plutovg_font_face_t* face);

    /**
     * @brief Decrements the reference count and potentially destroys the font face.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_font_face_destroy(plutovg_font_face_t* face);

    /**
     * @brief Retrieves the current reference count of a font face.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @return The reference count of the font face.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_font_face_get_reference_count(/*const*/ plutovg_font_face_t* face);

    /**
     * @brief Retrieves metrics for a font face at a specified size.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @param size The font size in pixels.
     * @param ascent Pointer to store the ascent metric.
     * @param descent Pointer to store the descent metric.
     * @param line_gap Pointer to store the line gap metric.
     * @param extents Pointer to a `plutovg_rect_t` object to store the font bounding box.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_font_face_get_metrics(/*const*/ plutovg_font_face_t* face, float size, float* ascent, float* descent, float* line_gap, plutovg_rect_t* extents);

    /**
     * @brief Retrieves metrics for a specified glyph at a given size.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @param size The font size in pixels.
     * @param codepoint The Unicode code point of the glyph.
     * @param advance_width Pointer to store the advance width of the glyph.
     * @param left_side_bearing Pointer to store the left side bearing of the glyph.
     * @param extents Pointer to a `plutovg_rect_t` object to store the glyph bounding box.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_font_face_get_glyph_metrics(/*const*/ plutovg_font_face_t* face, float size, plutovg_codepoint_t codepoint, float* advance_width, float* left_side_bearing, plutovg_rect_t* extents);

    /**
     * @brief Retrieves the path of a glyph and its advance width.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @param size The font size in pixels.
     * @param x The x-coordinate for positioning the glyph.
     * @param y The y-coordinate for positioning the glyph.
     * @param codepoint The Unicode code point of the glyph.
     * @param path Pointer to a `plutovg_path_t` object to store the glyph path.
     * @return The advance width of the glyph.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_font_face_get_glyph_path(/*const*/ plutovg_font_face_t* face, float size, float x, float y, plutovg_codepoint_t codepoint, plutovg_path_t* path);

    /**
     * @brief Traverses the path of a glyph and calls a callback for each path element.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @param size The font size in pixels.
     * @param x The x-coordinate for positioning the glyph.
     * @param y The y-coordinate for positioning the glyph.
     * @param codepoint The Unicode code point of the glyph.
     * @param traverse_func The callback function to be called for each path element.
     * @param closure User-defined data passed to the callback function.
     * @return The advance width of the glyph.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_font_face_traverse_glyph_path(/*const*/ plutovg_font_face_t* face, float size, float x, float y, plutovg_codepoint_t codepoint, plutovg_path_traverse_func_t traverse_func, void* closure);

    /**
     * @brief Computes the bounding box of a text string and its advance width.
     *
     * @param face A pointer to a `plutovg_font_face_t` object.
     * @param size The font size in pixels.
     * @param text Pointer to the text data.
     * @param length Length of the text data, or -1 if null-terminated.
     * @param encoding Encoding of the text data.
     * @param extents Pointer to a `plutovg_rect_t` object to store the bounding box of the text.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_font_face_text_extents(/*const*/ plutovg_font_face_t* face, float size, /*const*/ byte* text, int length, plutovg_text_encoding_t encoding, plutovg_rect_t* extents);

    /**
     * @brief Represents an image surface for drawing operations.
     *
     * The pixel data is stored in a premultiplied 32-bit ARGB format (0xAARRGGBB).
     * The red, green, and blue channels are multiplied by the alpha component divided by 255.
     * Premultiplied ARGB32 is beneficial for faster operations such as alpha blending.
     */
    public struct plutovg_surface_t
    {

    }

    /**
     * @brief Creates a new image surface with the specified dimensions.
     *
     * @param width The width of the surface in pixels.
     * @param height The height of the surface in pixels.
     * @return A pointer to the newly created `plutovg_surface_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_create(int width, int height);

    /**
     * @brief Creates an image surface using existing pixel data.
     *
     * @param data Pointer to the pixel data.
     * @param width The width of the surface in pixels.
     * @param height The height of the surface in pixels.
     * @param stride The number of bytes per row in the pixel data.
     * @return A pointer to the newly created `plutovg_surface_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_create_for_data(byte* data, int width, int height, int stride);

    /**
     * @brief Loads an image surface from a file.
     *
     * @param filename Path to the image file.
     * @return Pointer to the surface, or `NULL` on failure.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_load_from_image_file(/*const*/ char* filename);

    /**
     * @brief Loads an image surface from raw image data.
     *
     * @param data Pointer to the image data.
     * @param length Length of the data in bytes.
     * @return Pointer to the surface, or `NULL` on failure.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_load_from_image_data(/*const*/ void* data, int length);

    /**
     * @brief Loads an image surface from base64-encoded data.
     *
     * @param data Pointer to the base64-encoded image data.
     * @param length Length of the data in bytes, or `-1` if null-terminated.
     * @return Pointer to the surface, or `NULL` on failure.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_load_from_image_base64(/*const*/ char* data, int length);

    /**
     * @brief Increments the reference count for a surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return Pointer to the `plutovg_surface_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_surface_reference(plutovg_surface_t* surface);

    /**
     * @brief Decrements the reference count and destroys the surface if the count reaches zero.
     *
     * @param surface Pointer to the `plutovg_surface_t` object .
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_surface_destroy(plutovg_surface_t* surface);

    /**
     * @brief Gets the current reference count of a surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return The reference count of the surface.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_surface_get_reference_count(/*const*/ plutovg_surface_t* surface);

    /**
     * @brief Gets the pixel data of the surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return Pointer to the pixel data.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe byte* plutovg_surface_get_data(/*const*/ plutovg_surface_t* surface);

    /**
     * @brief Gets the width of the surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return Width of the surface in pixels.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_surface_get_width(/*const*/ plutovg_surface_t* surface);

    /**
     * @brief Gets the height of the surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return Height of the surface in pixels.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_surface_get_height(/*const*/ plutovg_surface_t* surface);

    /**
     * @brief Gets the stride of the surface.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @return Number of bytes per row.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_surface_get_stride(/*const*/ plutovg_surface_t* surface);

    /**
     * @brief Writes the surface to a PNG file.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @param filename Path to the output PNG file.
     * @return `true` if successful, `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_surface_write_to_png(/*const*/ plutovg_surface_t* surface, /*const*/ char* filename);

    /**
     * @brief Writes the surface to a JPEG file.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @param filename Path to the output JPEG file.
     * @param quality JPEG quality (0 to 100).
     * @return `true` if successful, `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_surface_write_to_jpg(/*const*/ plutovg_surface_t* surface, /*const*/ char* filename, int quality);

    /**
     * @brief Writes the surface to a PNG stream.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @param write_func Callback function for writing data.
     * @param closure User-defined data passed to the callback.
     * @return `true` if successful, `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_surface_write_to_png_stream(/*const*/ plutovg_surface_t* surface, plutovg_write_func_t write_func, void* closure);

    /**
     * @brief Writes the surface to a JPEG stream.
     *
     * @param surface Pointer to the `plutovg_surface_t` object.
     * @param write_func Callback function for writing data.
     * @param closure User-defined data passed to the callback.
     * @param quality JPEG quality (0 to 100).
     * @return `true` if successful, `false` otherwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe bool plutovg_surface_write_to_jpg_stream(/*const*/ plutovg_surface_t* surface, plutovg_write_func_t write_func, void* closure, int quality);

    /**
     * @brief Converts ARGB Premultiplied to RGBA Plain.
     *
     * @param dst Destination buffer (can be the same as `src`).
     * @param src Source buffer (ARGB Premultiplied).
     * @param width Image width in pixels.
     * @param height Image height in pixels.
     * @param stride Image stride in bytes.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_convert_argb_to_rgba(byte* dst, /*const*/ byte* src, int width, int height, int stride);

    /**
     * @brief Converts RGBA Plain to ARGB Premultiplied.
     *
     * @param dst Destination buffer (can be the same as `src`).
     * @param src Source buffer (RGBA Plain).
     * @param width Image width in pixels.
     * @param height Image height in pixels.
     * @param stride Image stride in bytes.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_convert_rgba_to_argb(byte* dst, /*const*/ byte* src, int width, int height, int stride);

    /**
     * @brief Represents a color with red, green, blue, and alpha components.
     */
    public struct plutovg_color_t {
        public float r; ///< Red component (0 to 1).
        public float g; ///< Green component (0 to 1).
        public float b; ///< Blue component (0 to 1).
        public float a; ///< Alpha (opacity) component (0 to 1).
    }

    /**
     * @brief Defines the type of texture, either plain or tiled.
     */
    public enum plutovg_texture_type_t {
        PLUTOVG_TEXTURE_TYPE_PLAIN, ///< Plain texture.
        PLUTOVG_TEXTURE_TYPE_TILED ///< Tiled texture.
    }

    /**
     * @brief Defines the spread method for gradients.
     */
    public enum plutovg_spread_method_t {
        PLUTOVG_SPREAD_METHOD_PAD, ///< Pad the gradient's edges.
        PLUTOVG_SPREAD_METHOD_REFLECT, ///< Reflect the gradient beyond its bounds.
        PLUTOVG_SPREAD_METHOD_REPEAT ///< Repeat the gradient pattern.
    }

    /**
     * @brief Represents a gradient stop.
     */
    public struct plutovg_gradient_stop_t {
        public float offset; ///< The offset of the gradient stop, as a value between 0 and 1.
        public plutovg_color_t color; ///< The color of the gradient stop.
    }

    /**
     * @brief Represents a paint object used for drawing operations.
     */
    public struct plutovg_paint_t
    {

    }

    /**
     * @brief Creates a solid RGB paint.
     *
     * @param r The red component (0 to 1).
     * @param g The green component (0 to 1).
     * @param b The blue component (0 to 1).
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_rgb(float r, float g, float b);

    /**
     * @brief Creates a solid RGBA paint.
     *
     * @param r The red component (0 to 1).
     * @param g The green component (0 to 1).
     * @param b The blue component (0 to 1).
     * @param a The alpha component (0 to 1).
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_rgba(float r, float g, float b, float a);

    /**
     * @brief Creates a solid color paint.
     *
     * @param color A pointer to the `plutovg_color_t` object.
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_color(/*const*/ plutovg_color_t* color);

    /**
     * @brief Creates a linear gradient paint.
     *
     * @param x1 The x coordinate of the gradient start.
     * @param y1 The y coordinate of the gradient start.
     * @param x2 The x coordinate of the gradient end.
     * @param y2 The y coordinate of the gradient end.
     * @param spread The gradient spread method.
     * @param stops Array of gradient stops.
     * @param nstops Number of gradient stops.
     * @param matrix Optional transformation matrix.
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_linear_gradient(float x1, float y1, float x2, float y2,
                                                                      plutovg_spread_method_t spread, /*const*/ plutovg_gradient_stop_t* stops, int nstops, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Creates a radial gradient paint.
     *
     * @param cx The x coordinate of the gradient center.
     * @param cy The y coordinate of the gradient center.
     * @param cr The radius of the gradient.
     * @param fx The x coordinate of the focal point.
     * @param fy The y coordinate of the focal point.
     * @param fr The radius of the focal point.
     * @param spread The gradient spread method.
     * @param stops Array of gradient stops.
     * @param nstops Number of gradient stops.
     * @param matrix Optional transformation matrix.
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_radial_gradient(float cx, float cy, float cr, float fx, float fy, float fr,
                                                                      plutovg_spread_method_t spread, /*const*/ plutovg_gradient_stop_t* stops, int nstops, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Creates a texture paint from a surface.
     *
     * @param surface The texture surface.
     * @param type The texture type (plain or tiled).
     * @param opacity The opacity of the texture (0 to 1).
     * @param matrix Optional transformation matrix.
     * @return A pointer to the created `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_create_texture(plutovg_surface_t* surface, plutovg_texture_type_t type, float opacity, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Increments the reference count of a paint object.
     *
     * @param paint A pointer to the `plutovg_paint_t` object.
     * @return A pointer to the referenced `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_paint_reference(plutovg_paint_t* paint);

    /**
     * @brief Decrements the reference count and destroys the paint if the count reaches zero.
     *
     * @param paint A pointer to the `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_paint_destroy(plutovg_paint_t* paint);

    /**
     * @brief Retrieves the reference count of a paint object.
     *
     * @param paint A pointer to the `plutovg_paint_t` object.
     * @return The reference count of the `plutovg_paint_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_paint_get_reference_count(/*const*/ plutovg_paint_t* paint);

    /**
     * @brief Defines fill rule types for filling paths.
     */
    public enum plutovg_fill_rule_t {
        PLUTOVG_FILL_RULE_NON_ZERO, ///< Non-zero winding fill rule.
        PLUTOVG_FILL_RULE_EVEN_ODD ///< Even-odd fill rule.
    }

    /**
     * @brief Defines compositing operations.
     */
    public enum plutovg_operator_t {
        PLUTOVG_OPERATOR_SRC, ///< Source replaces destination.
        PLUTOVG_OPERATOR_SRC_OVER, ///< Source over destination.
        PLUTOVG_OPERATOR_DST_IN, ///< Destination within source.
        PLUTOVG_OPERATOR_DST_OUT ///< Destination outside source.
    }

    /**
     * @brief Defines the shape used at the ends of open subpaths.
     */
    public enum plutovg_line_cap_t {
        PLUTOVG_LINE_CAP_BUTT, ///< Flat edge at the end of the stroke.
        PLUTOVG_LINE_CAP_ROUND, ///< Rounded ends at the end of the stroke.
        PLUTOVG_LINE_CAP_SQUARE ///< Square ends at the end of the stroke.
    }

    /**
     * @brief Defines the shape used at the corners of paths.
     */
    public enum plutovg_line_join_t {
        PLUTOVG_LINE_JOIN_MITER, ///< Miter join with sharp corners.
        PLUTOVG_LINE_JOIN_ROUND, ///< Rounded join.
        PLUTOVG_LINE_JOIN_BEVEL ///< Beveled join with a flattened corner.
    }

    /**
     * @brief Represents a drawing context.
     */
    public struct plutovg_canvas_t
    {

    }

    /**
     * @brief Creates a drawing context on a surface.
     *
     * @param surface A pointer to a `plutovg_surface_t` object.
     * @return A pointer to the newly created `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_canvas_t* plutovg_canvas_create(plutovg_surface_t* surface);

    /**
     * @brief Increases the reference count of the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The same pointer to the `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_canvas_t* plutovg_canvas_reference(plutovg_canvas_t* canvas);

    /**
     * @brief Decreases the reference count and destroys the canvas when it reaches zero.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_destroy(plutovg_canvas_t* canvas);

    /**
     * @brief Retrieves the reference count of the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current reference count.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_canvas_get_reference_count(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Gets the surface associated with the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return A pointer to the `plutovg_surface_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_surface_t* plutovg_canvas_get_surface(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Saves the current state of the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_save(plutovg_canvas_t* canvas);

    /**
     * @brief Restores the canvas to the most recently saved state.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_restore(plutovg_canvas_t* canvas);

    /**
     * @brief Sets the current paint to a solid color.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param r The red component (0 to 1).
     * @param g The green component (0 to 1).
     * @param b The blue component (0 to 1).
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_rgb(plutovg_canvas_t* canvas, float r, float g, float b);

    /**
     * @brief Sets the current paint to a solid color.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param r The red component (0 to 1).
     * @param g The green component (0 to 1).
     * @param b The blue component (0 to 1).
     * @param a The alpha component (0 to 1).
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_rgba(plutovg_canvas_t* canvas, float r, float g, float b, float a);

    /**
     * @brief Sets the current paint to a solid color.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param color A pointer to a `plutovg_color_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_color(plutovg_canvas_t* canvas, /*const*/ plutovg_color_t* color);

    /**
     * @brief Sets the current paint to a linear gradient.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x1 The x coordinate of the start point.
     * @param y1 The y coordinate of the start point.
     * @param x2 The x coordinate of the end point.
     * @param y2 The y coordinate of the end point.
     * @param spread The gradient spread method.
     * @param stops Array of gradient stops.
     * @param nstops Number of gradient stops.
     * @param matrix Optional transformation matrix.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_linear_gradient(plutovg_canvas_t* canvas, float x1, float y1, float x2, float y2,
                                                        plutovg_spread_method_t spread, /*const*/ plutovg_gradient_stop_t* stops, int nstops, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Sets the current paint to a radial gradient.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param cx The x coordinate of the center.
     * @param cy The y coordinate of the center.
     * @param cr The radius of the gradient.
     * @param fx The x coordinate of the focal point.
     * @param fy The y coordinate of the focal point.
     * @param fr The radius of the focal point.
     * @param spread The gradient spread method.
     * @param stops Array of gradient stops.
     * @param nstops Number of gradient stops.
     * @param matrix Optional transformation matrix.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_radial_gradient(plutovg_canvas_t* canvas, float cx, float cy, float cr, float fx, float fy, float fr,
                                                        plutovg_spread_method_t spread, /*const*/ plutovg_gradient_stop_t* stops, int nstops, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Sets the current paint to a texture.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param surface The texture surface.
     * @param type The texture type (plain or tiled).
     * @param opacity The opacity of the texture (0 to 1).
     * @param matrix Optional transformation matrix.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_texture(plutovg_canvas_t* canvas, plutovg_surface_t* surface, plutovg_texture_type_t type, float opacity, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Sets the current paint.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param paint The paint to be used for subsequent drawing operations.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_paint(plutovg_canvas_t* canvas, plutovg_paint_t* paint);

    /**
     * @brief Retrieves the current paint.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current paint used for drawing operations.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_paint_t* plutovg_canvas_get_paint(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the font face and size for text rendering on the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param face A pointer to a `plutovg_font_face_t` object representing the font face to use.
     * @param size The size of the font, in pixels. This determines the height of the rendered text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_font(plutovg_canvas_t* canvas, plutovg_font_face_t* face, float size);

    /**
     * @brief Sets the font face for text rendering on the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param face A pointer to a `plutovg_font_face_t` object representing the font face to use.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_font_face(plutovg_canvas_t* canvas, plutovg_font_face_t* face);

    /**
     * @brief Retrieves the current font face used for text rendering on the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return A pointer to a `plutovg_font_face_t` object representing the current font face.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_font_face_t* plutovg_canvas_get_font_face(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the font size for text rendering on the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param size The size of the font, in pixels. This value defines the height of the rendered text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_font_size(plutovg_canvas_t* canvas, float size);

    /**
     * @brief Retrieves the current font size used for text rendering on the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current font size, in pixels. This value represents the height of the rendered text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_get_font_size(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the fill rule.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param winding The fill rule.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_fill_rule(plutovg_canvas_t* canvas, plutovg_fill_rule_t winding);

    /**
     * @brief Retrieves the current fill rule.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current fill rule.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_fill_rule_t plutovg_canvas_get_fill_rule(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the compositing operator.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param op The compositing operator.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_operator(plutovg_canvas_t* canvas, plutovg_operator_t op);

    /**
     * @brief Retrieves the current compositing operator.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current compositing operator.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_operator_t plutovg_canvas_get_operator(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the global opacity.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param opacity The opacity value (0 to 1).
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_opacity(plutovg_canvas_t* canvas, float opacity);

    /**
     * @brief Retrieves the current global opacity.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current opacity value.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_get_opacity(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the line width.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param line_width The width of the stroke.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_line_width(plutovg_canvas_t* canvas, float line_width);

    /**
     * @brief Retrieves the current line width.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current line width.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_get_line_width(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the line cap style.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param line_cap The line cap style.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_line_cap(plutovg_canvas_t* canvas, plutovg_line_cap_t line_cap);

    /**
     * @brief Retrieves the current line cap style.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current line cap style.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_line_cap_t plutovg_canvas_get_line_cap(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the line join style.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param line_join The line join style.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_line_join(plutovg_canvas_t* canvas, plutovg_line_join_t line_join);

    /**
     * @brief Retrieves the current line join style.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current line join style.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_line_join_t plutovg_canvas_get_line_join(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the miter limit.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param miter_limit The miter limit value.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_miter_limit(plutovg_canvas_t* canvas, float miter_limit);

    /**
     * @brief Retrieves the current miter limit.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current miter limit value.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_get_miter_limit(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the dash pattern.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param offset The dash offset.
     * @param dashes Array of dash lengths.
     * @param ndashes Number of dash lengths.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_dash(plutovg_canvas_t* canvas, float offset, /*const*/ float* dashes, int ndashes);

    /**
     * @brief Sets the dash offset.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param offset The dash offset.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_dash_offset(plutovg_canvas_t* canvas, float offset);

    /**
     * @brief Retrieves the current dash offset.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current dash offset.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_get_dash_offset(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Sets the dash pattern.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param dashes Array of dash lengths.
     * @param ndashes Number of dash lengths.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_dash_array(plutovg_canvas_t* canvas, /*const*/ float* dashes, int ndashes);

    /**
     * @brief Retrieves the current dash pattern.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param dashes Pointer to store the dash array.
     * @return The number of dash lengths.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int plutovg_canvas_get_dash_array(/*const*/ plutovg_canvas_t* canvas, /*const*/ float** dashes);

    /**
     * @brief Translates the current transformation matrix by offsets `tx` and `ty`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param tx The translation offset in the x-direction.
     * @param ty The translation offset in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_translate(plutovg_canvas_t* canvas, float tx, float ty);

    /**
     * @brief Scales the current transformation matrix by factors `sx` and `sy`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param sx The scaling factor in the x-direction.
     * @param sy The scaling factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_scale(plutovg_canvas_t* canvas, float sx, float sy);

    /**
     * @brief Shears the current transformation matrix by factors `shx` and `shy`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param shx The shearing factor in the x-direction.
     * @param shy The shearing factor in the y-direction.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_shear(plutovg_canvas_t* canvas, float shx, float shy);

    /**
     * @brief Rotates the current transformation matrix by the specified angle (in radians).
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param angle The rotation angle in radians.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_rotate(plutovg_canvas_t* canvas, float angle);

    /**
     * @brief Multiplies the current transformation matrix with the specified `matrix`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param matrix A pointer to the `plutovg_matrix_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_transform(plutovg_canvas_t* canvas, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Resets the current transformation matrix to the identity matrix.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_reset_matrix(plutovg_canvas_t* canvas);

    /**
     * @brief Resets the current transformation matrix to the specified `matrix`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param matrix A pointer to the `plutovg_matrix_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_set_matrix(plutovg_canvas_t* canvas, /*const*/ plutovg_matrix_t* matrix);

    /**
     * @brief Stores the current transformation matrix in `matrix`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param A pointer to the `plutovg_matrix_t` to store the matrix.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_get_matrix(/*const*/ plutovg_canvas_t* canvas, plutovg_matrix_t* matrix);

    /**
     * @brief Transforms the point `(x, y)` using the current transformation matrix and stores the result in `(xx, yy)`.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the point to transform.
     * @param y The y-coordinate of the point to transform.
     * @param xx A pointer to store the transformed x-coordinate.
     * @param yy A pointer to store the transformed y-coordinate.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_map(/*const*/ plutovg_canvas_t* canvas, float x, float y, float* xx, float* yy);

    /**
     * @brief Transforms the `src` point using the current transformation matrix and stores the result in `dst`.
     * @note `src` and `dst` can be identical.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param src A pointer to the `plutovg_point_t` point to transform.
     * @param dst A pointer to the `plutovg_point_t` to store the transformed point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_map_point(/*const*/ plutovg_canvas_t* canvas, /*const*/ plutovg_point_t* src, plutovg_point_t* dst);

    /**
     * @brief Transforms the `src` rectangle using the current transformation matrix and stores the result in `dst`.
     * @note `src` and `dst` can be identical.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param src A pointer to the `plutovg_rect_t` rectangle to transform.
     * @param dst A pointer to the `plutovg_rect_t` to store the transformed rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_map_rect(/*const*/ plutovg_canvas_t* canvas, /*const*/ plutovg_rect_t* src, plutovg_rect_t* dst);

    /**
     * @brief Moves the current point to a new position.
     *
     * Moves the current point to the specified coordinates without adding a line.
     * This operation is added to the current path. Equivalent to the SVG `M` command.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the new position.
     * @param y The y-coordinate of the new position.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_move_to(plutovg_canvas_t* canvas, float x, float y);

    /**
     * @brief Adds a straight line segment to the current path.
     *
     * Adds a straight line from the current point to the specified coordinates.
     * This segment is added to the current path. Equivalent to the SVG `L` command.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the end point of the line.
     * @param y The y-coordinate of the end point of the line.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_line_to(plutovg_canvas_t* canvas, float x, float y);

    /**
     * @brief Adds a quadratic Bézier curve to the current path.
     *
     * Adds a quadratic Bézier curve from the current point to the specified end point,
     * using the given control point. This curve is added to the current path. Equivalent to the SVG `Q` command.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x1 The x-coordinate of the control point.
     * @param y1 The y-coordinate of the control point.
     * @param x2 The x-coordinate of the end point of the curve.
     * @param y2 The y-coordinate of the end point of the curve.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_quad_to(plutovg_canvas_t* canvas, float x1, float y1, float x2, float y2);

    /**
     * @brief Adds a cubic Bézier curve to the current path.
     *
     * Adds a cubic Bézier curve from the current point to the specified end point,
     * using the given control points. This curve is added to the current path. Equivalent to the SVG `C` command.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x1 The x-coordinate of the first control point.
     * @param y1 The y-coordinate of the first control point.
     * @param x2 The x-coordinate of the second control point.
     * @param y2 The y-coordinate of the second control point.
     * @param x3 The x-coordinate of the end point of the curve.
     * @param y3 The y-coordinate of the end point of the curve.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_cubic_to(plutovg_canvas_t* canvas, float x1, float y1, float x2, float y2, float x3, float y3);

    /**
     * @brief Adds an elliptical arc to the current path.
     *
     * Adds an elliptical arc from the current point to the specified end point,
     * defined by radii, rotation angle, and flags for arc type and direction.
     * This arc segment is added to the current path. Equivalent to the SVG `A` command.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param rx The x-radius of the ellipse.
     * @param ry The y-radius of the ellipse.
     * @param angle The rotation angle of the ellipse in degrees.
     * @param large_arc_flag If true, add the large arc; otherwise, add the small arc.
     * @param sweep_flag If true, add the arc in the positive-angle direction; otherwise, in the negative-angle direction.
     * @param x The x-coordinate of the end point.
     * @param y The y-coordinate of the end point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_arc_to(plutovg_canvas_t* canvas, float rx, float ry, float angle, bool large_arc_flag, bool sweep_flag, float x, float y);

    /**
     * @brief Adds a rectangle to the current path.
     *
     * Adds a rectangle with the specified position and dimensions to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the rectangle's origin.
     * @param y The y-coordinate of the rectangle's origin.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_rect(plutovg_canvas_t* canvas, float x, float y, float w, float h);

    /**
     * @brief Adds a rounded rectangle to the current path.
     *
     * Adds a rectangle with rounded corners defined by the specified position,
     * dimensions, and corner radii to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the rectangle's origin.
     * @param y The y-coordinate of the rectangle's origin.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     * @param rx The x-radius of the corners.
     * @param ry The y-radius of the corners.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_round_rect(plutovg_canvas_t* canvas, float x, float y, float w, float h, float rx, float ry);

    /**
     * @brief Adds an ellipse to the current path.
     *
     * Adds an ellipse centered at the specified coordinates with the given radii to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param cx The x-coordinate of the ellipse's center.
     * @param cy The y-coordinate of the ellipse's center.
     * @param rx The x-radius of the ellipse.
     * @param ry The y-radius of the ellipse.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_ellipse(plutovg_canvas_t* canvas, float cx, float cy, float rx, float ry);

    /**
     * @brief Adds a circle to the current path.
     *
     * Adds a circle centered at the specified coordinates with the given radius to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param cx The x-coordinate of the circle's center.
     * @param cy The y-coordinate of the circle's center.
     * @param r The radius of the circle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_circle(plutovg_canvas_t* canvas, float cx, float cy, float r);

    /**
     * @brief Adds an arc to the current path.
     *
     * Adds an arc centered at the specified coordinates, with a given radius,
     * starting and ending at the specified angles. The direction of the arc is
     * determined by `ccw`. This arc segment is added to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param cx The x-coordinate of the arc's center.
     * @param cy The y-coordinate of the arc's center.
     * @param r The radius of the arc.
     * @param a0 The starting angle of the arc in radians.
     * @param a1 The ending angle of the arc in radians.
     * @param ccw If true, add the arc counter-clockwise; otherwise, clockwise.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_arc(plutovg_canvas_t* canvas, float cx, float cy, float r, float a0, float a1, bool ccw);

    /**
     * @brief Adds a path to the current path.
     *
     * Appends the elements of the specified path to the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param path A pointer to the `plutovg_path_t` object to be added.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_add_path(plutovg_canvas_t* canvas, /*const*/ plutovg_path_t* path);

    /**
     * @brief Starts a new path on the canvas.
     *
     * Begins a new path, clearing any existing path data. The new path starts with no commands.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_new_path(plutovg_canvas_t* canvas);

    /**
     * @brief Closes the current path.
     *
     * Closes the current path by adding a straight line back to the starting point.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_close_path(plutovg_canvas_t* canvas);

    /**
     * @brief Retrieves the current point of the canvas.
     *
     * Gets the coordinates of the current point in the canvas, which is the last point
     * added or moved to in the current path.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the current point.
     * @param y The y-coordinate of the current point.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_get_current_point(/*const*/ plutovg_canvas_t* canvas, float* x, float* y);

    /**
     * @brief Gets the current path from the canvas.
     *
     * Retrieves the path object representing the sequence of path commands added to the canvas.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @return The current path.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe plutovg_path_t* plutovg_canvas_get_path(/*const*/ plutovg_canvas_t* canvas);

    /**
     * @brief Gets the bounding box of the filled region.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param extents The bounding box of the filled region.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_fill_extents(/*const*/ plutovg_canvas_t* canvas, plutovg_rect_t* extents);

    /**
     * @brief Gets the bounding box of the stroked region.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param extents The bounding box of the stroked region.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_stroke_extents(/*const*/ plutovg_canvas_t* canvas, plutovg_rect_t* extents);

    /**
     * @brief Gets the bounding box of the clipped region.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param extents The bounding box of the clipped region.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_clip_extents(/*const*/ plutovg_canvas_t* canvas, plutovg_rect_t* extents);

    /**
     * @brief A drawing operator that fills the current path according to the current fill rule.
     *
     * The current path will be cleared after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_fill(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that strokes the current path according to the current stroke settings.
     *
     * The current path will be cleared after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_stroke(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that intersects the current clipping region with the current path according to the current fill rule.
     *
     * The current path will be cleared after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_clip(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that paints the current clipping region using the current paint.
     *
     * @note The current path will not be affected by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_paint(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that fills the current path according to the current fill rule.
     *
     * The current path will be preserved after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_fill_preserve(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that strokes the current path according to the current stroke settings.
     *
     * The current path will be preserved after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_stroke_preserve(plutovg_canvas_t* canvas);

    /**
     * @brief A drawing operator that intersects the current clipping region with the current path according to the current fill rule.
     *
     * The current path will be preserved after this operation.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_clip_preserve(plutovg_canvas_t* canvas);

    /**
     * @brief Fills a rectangle according to the current fill rule.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the rectangle's origin.
     * @param y The y-coordinate of the rectangle's origin.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_fill_rect(plutovg_canvas_t* canvas, float x, float y, float w, float h);

    /**
     * @brief Fills a path according to the current fill rule.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param path The `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_fill_path(plutovg_canvas_t* canvas, /*const*/ plutovg_path_t* path);

    /**
     * @brief Strokes a rectangle with the current stroke settings.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the rectangle's origin.
     * @param y The y-coordinate of the rectangle's origin.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_stroke_rect(plutovg_canvas_t* canvas, float x, float y, float w, float h);

    /**
     * @brief Strokes a path with the current stroke settings.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param path The `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_stroke_path(plutovg_canvas_t* canvas, /*const*/ plutovg_path_t* path);

    /**
     * @brief Intersects the current clipping region with a rectangle according to the current fill rule.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param x The x-coordinate of the rectangle's origin.
     * @param y The y-coordinate of the rectangle's origin.
     * @param w The width of the rectangle.
     * @param h The height of the rectangle.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_clip_rect(plutovg_canvas_t* canvas, float x, float y, float w, float h);

    /**
     * @brief Intersects the current clipping region with a path according to the current fill rule.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param path The `plutovg_path_t` object.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_clip_path(plutovg_canvas_t* canvas, /*const*/ plutovg_path_t* path);

    /**
     * @brief Adds a glyph to the current path at the specified origin.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param codepoint The glyph codepoint.
     * @param x The x-coordinate of the origin.
     * @param y The y-coordinate of the origin.
     * @return The advance width of the glyph.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_add_glyph(plutovg_canvas_t* canvas, plutovg_codepoint_t codepoint, float x, float y);

    /**
     * @brief Adds text to the current path at the specified origin.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param text The text data.
     * @param length The length of the text data, or -1 if null-terminated.
     * @param encoding The encoding of the text data.
     * @param x The x-coordinate of the origin.
     * @param y The y-coordinate of the origin.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_add_text(plutovg_canvas_t* canvas, /*const*/ byte* text, int length, plutovg_text_encoding_t encoding, float x, float y);

    /**
     * @brief Fills a text at the specified origin.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param text The text data.
     * @param length The length of the text data, or -1 if null-terminated.
     * @param encoding The encoding of the text data.
     * @param x The x-coordinate of the origin.
     * @param y The y-coordinate of the origin.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_fill_text(plutovg_canvas_t* canvas, /*const*/ byte* text, int length, plutovg_text_encoding_t encoding, float x, float y);

    /**
     * @brief Strokes a text at the specified origin.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param text The text data.
     * @param length The length of the text data, or -1 if null-terminated.
     * @param encoding The encoding of the text data.
     * @param x The x-coordinate of the origin.
     * @param y The y-coordinate of the origin.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_stroke_text(plutovg_canvas_t* canvas, /*const*/ byte* text, int length, plutovg_text_encoding_t encoding, float x, float y);

    /**
     * @brief Intersects the current clipping region with text at the specified origin.
     *
     * @note The current path will be cleared by this operation.
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param text The text data.
     * @param length The length of the text data, or -1 if null-terminated.
     * @param encoding The encoding of the text data.
     * @param x The x-coordinate of the origin.
     * @param y The y-coordinate of the origin.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_clip_text(plutovg_canvas_t* canvas, /*const*/ byte* text, int length, plutovg_text_encoding_t encoding, float x, float y);

    /**
     * @brief Retrieves font metrics for the current font.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param ascent The ascent of the font.
     * @param descent The descent of the font.
     * @param line_gap The line gap of the font.
     * @param extents The bounding box of the font.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_font_metrics(plutovg_canvas_t* canvas, float* ascent, float* descent, float* line_gap, plutovg_rect_t* extents);

    /**
     * @brief Retrieves metrics for a specific glyph.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param codepoint The glyph codepoint.
     * @param advance_width The advance width of the glyph.
     * @param left_side_bearing The left side bearing of the glyph.
     * @param extents The bounding box of the glyph.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void plutovg_canvas_glyph_metrics(plutovg_canvas_t* canvas, plutovg_codepoint_t codepoint, float* advance_width, float* left_side_bearing, plutovg_rect_t* extents);

    /**
     * @brief Retrieves the extents of a text.
     *
     * @param canvas A pointer to a `plutovg_canvas_t` object.
     * @param text The text data.
     * @param length The length of the text data, or -1 if null-terminated.
     * @param encoding The encoding of the text data.
     * @param extents The bounding box of the text.
     * @return The total advance width of the text.
     */
    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe float plutovg_canvas_text_extents(plutovg_canvas_t* canvas, /*const*/ void* text, int length, plutovg_text_encoding_t encoding, plutovg_rect_t* extents);

}
