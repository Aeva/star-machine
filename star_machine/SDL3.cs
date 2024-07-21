
// The contents of this file is largely kitbashed from these two repositories,
// and their respective copyrights and licenses probably also apply here:
//  - https://github.com/flibitijibibo/SDL2-CS/
//  - https://github.com/thatcosmonaut/SDL/tree/gpu/include/SDL3

using System;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Text;

using SDL_bool = int;
using size_t = nint;

using SDL_GpuTextureUsageFlags = System.UInt32;
using SDL_GpuBufferUsageFlags = System.UInt32;
using SDL_GpuColorComponentFlags = System.UInt32;

using SDL_GpuBackend = System.UInt64;

using SDL_Window_Ptr = System.IntPtr;
using SDL_IOStream_Ptr = System.IntPtr;
using SDL_Gamepad_Ptr = System.IntPtr;
using SDL_Surface_Ptr = System.IntPtr;
using SDL_GpuDevice_Ptr = System.IntPtr;
using SDL_GpuBuffer_Ptr = System.IntPtr;
using SDL_GpuTransferBuffer_Ptr = System.IntPtr;
using SDL_GpuTexture_Ptr = System.IntPtr;
using SDL_GpuSampler_Ptr = System.IntPtr;
using SDL_GpuShader_Ptr = System.IntPtr;
using SDL_GpuComputePipeline_Ptr = System.IntPtr;
using SDL_GpuGraphicsPipeline_Ptr = System.IntPtr;
using SDL_GpuCommandBuffer_Ptr = System.IntPtr;
using SDL_GpuRenderPass_Ptr = System.IntPtr;
using SDL_GpuComputePass_Ptr = System.IntPtr;
using SDL_GpuCopyPass_Ptr = System.IntPtr;
using SDL_GpuFence_Ptr = System.IntPtr;

namespace SDL3
{
    public static class SDL
    {
        private const string nativeLibName = "SDL3";

        #region SDL_init.h
        public const uint SDL_INIT_TIMER = 0x00000001u;
        public const uint SDL_INIT_AUDIO = 0x00000010u; // < `SDL_INIT_AUDIO` implies `SDL_INIT_EVENTS`
        public const uint SDL_INIT_VIDEO = 0x00000020u; // < `SDL_INIT_VIDEO` implies `SDL_INIT_EVENTS`
        public const uint SDL_INIT_JOYSTICK = 0x00000200u; // < `SDL_INIT_JOYSTICK` implies `SDL_INIT_EVENTS`, should be initialized on the same thread as SDL_INIT_VIDEO on Windows if you don't set SDL_HINT_JOYSTICK_THREAD
        public const uint SDL_INIT_HAPTIC = 0x00001000u;
        public const uint SDL_INIT_GAMEPAD = 0x00002000u; // < `SDL_INIT_GAMEPAD` implies `SDL_INIT_JOYSTICK`
        public const uint SDL_INIT_EVENTS = 0x00004000u;
        public const uint SDL_INIT_SENSOR = 0x00008000u; // < `SDL_INIT_SENSOR` implies `SDL_INIT_EVENTS`
        public const uint SDL_INIT_CAMERA = 0x00010000u; // < `SDL_INIT_CAMERA` implies `SDL_INIT_EVENTS`

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_Init(uint flags);
        #endregion

        #region SDL_scancode.h
        /**
         * The SDL keyboard scancode representation.
         *
         * An SDL scancode is the physical representation of a key on the keyboard,
         * independent of language and keyboard mapping.
         *
         * Values of this type are used to represent keyboard keys, among other places
         * in the `scancode` field of the SDL_KeyboardEvent structure.
         *
         * The values in this enumeration are based on the USB usage page standard:
         * https://usb.org/sites/default/files/hut1_5.pdf
         *
         * \since This enum is available since SDL 3.0.0.
         */
        public enum SDL_Scancode
        {
            SDL_SCANCODE_UNKNOWN = 0,

            /**
             *  \name Usage page 0x07
             *
             *  These values are from usage page 0x07 (USB keyboard page).
             */
            /* @{ */

            SDL_SCANCODE_A = 4,
            SDL_SCANCODE_B = 5,
            SDL_SCANCODE_C = 6,
            SDL_SCANCODE_D = 7,
            SDL_SCANCODE_E = 8,
            SDL_SCANCODE_F = 9,
            SDL_SCANCODE_G = 10,
            SDL_SCANCODE_H = 11,
            SDL_SCANCODE_I = 12,
            SDL_SCANCODE_J = 13,
            SDL_SCANCODE_K = 14,
            SDL_SCANCODE_L = 15,
            SDL_SCANCODE_M = 16,
            SDL_SCANCODE_N = 17,
            SDL_SCANCODE_O = 18,
            SDL_SCANCODE_P = 19,
            SDL_SCANCODE_Q = 20,
            SDL_SCANCODE_R = 21,
            SDL_SCANCODE_S = 22,
            SDL_SCANCODE_T = 23,
            SDL_SCANCODE_U = 24,
            SDL_SCANCODE_V = 25,
            SDL_SCANCODE_W = 26,
            SDL_SCANCODE_X = 27,
            SDL_SCANCODE_Y = 28,
            SDL_SCANCODE_Z = 29,

            SDL_SCANCODE_1 = 30,
            SDL_SCANCODE_2 = 31,
            SDL_SCANCODE_3 = 32,
            SDL_SCANCODE_4 = 33,
            SDL_SCANCODE_5 = 34,
            SDL_SCANCODE_6 = 35,
            SDL_SCANCODE_7 = 36,
            SDL_SCANCODE_8 = 37,
            SDL_SCANCODE_9 = 38,
            SDL_SCANCODE_0 = 39,

            SDL_SCANCODE_RETURN = 40,
            SDL_SCANCODE_ESCAPE = 41,
            SDL_SCANCODE_BACKSPACE = 42,
            SDL_SCANCODE_TAB = 43,
            SDL_SCANCODE_SPACE = 44,

            SDL_SCANCODE_MINUS = 45,
            SDL_SCANCODE_EQUALS = 46,
            SDL_SCANCODE_LEFTBRACKET = 47,
            SDL_SCANCODE_RIGHTBRACKET = 48,
            SDL_SCANCODE_BACKSLASH = 49, /**< Located at the lower left of the return
                                  *   key on ISO keyboards and at the right end
                                  *   of the QWERTY row on ANSI keyboards.
                                  *   Produces REVERSE SOLIDUS (backslash) and
                                  *   VERTICAL LINE in a US layout, REVERSE
                                  *   SOLIDUS and VERTICAL LINE in a UK Mac
                                  *   layout, NUMBER SIGN and TILDE in a UK
                                  *   Windows layout, DOLLAR SIGN and POUND SIGN
                                  *   in a Swiss German layout, NUMBER SIGN and
                                  *   APOSTROPHE in a German layout, GRAVE
                                  *   ACCENT and POUND SIGN in a French Mac
                                  *   layout, and ASTERISK and MICRO SIGN in a
                                  *   French Windows layout.
                                  */
            SDL_SCANCODE_NONUSHASH = 50, /**< ISO USB keyboards actually use this code
                                  *   instead of 49 for the same key, but all
                                  *   OSes I've seen treat the two codes
                                  *   identically. So, as an implementor, unless
                                  *   your keyboard generates both of those
                                  *   codes and your OS treats them differently,
                                  *   you should generate SDL_SCANCODE_BACKSLASH
                                  *   instead of this code. As a user, you
                                  *   should not rely on this code because SDL
                                  *   will never generate it with most (all?)
                                  *   keyboards.
                                  */
            SDL_SCANCODE_SEMICOLON = 51,
            SDL_SCANCODE_APOSTROPHE = 52,
            SDL_SCANCODE_GRAVE = 53, /**< Located in the top left corner (on both ANSI
                              *   and ISO keyboards). Produces GRAVE ACCENT and
                              *   TILDE in a US Windows layout and in US and UK
                              *   Mac layouts on ANSI keyboards, GRAVE ACCENT
                              *   and NOT SIGN in a UK Windows layout, SECTION
                              *   SIGN and PLUS-MINUS SIGN in US and UK Mac
                              *   layouts on ISO keyboards, SECTION SIGN and
                              *   DEGREE SIGN in a Swiss German layout (Mac:
                              *   only on ISO keyboards), CIRCUMFLEX ACCENT and
                              *   DEGREE SIGN in a German layout (Mac: only on
                              *   ISO keyboards), SUPERSCRIPT TWO and TILDE in a
                              *   French Windows layout, COMMERCIAL AT and
                              *   NUMBER SIGN in a French Mac layout on ISO
                              *   keyboards, and LESS-THAN SIGN and GREATER-THAN
                              *   SIGN in a Swiss German, German, or French Mac
                              *   layout on ANSI keyboards.
                              */
            SDL_SCANCODE_COMMA = 54,
            SDL_SCANCODE_PERIOD = 55,
            SDL_SCANCODE_SLASH = 56,

            SDL_SCANCODE_CAPSLOCK = 57,

            SDL_SCANCODE_F1 = 58,
            SDL_SCANCODE_F2 = 59,
            SDL_SCANCODE_F3 = 60,
            SDL_SCANCODE_F4 = 61,
            SDL_SCANCODE_F5 = 62,
            SDL_SCANCODE_F6 = 63,
            SDL_SCANCODE_F7 = 64,
            SDL_SCANCODE_F8 = 65,
            SDL_SCANCODE_F9 = 66,
            SDL_SCANCODE_F10 = 67,
            SDL_SCANCODE_F11 = 68,
            SDL_SCANCODE_F12 = 69,

            SDL_SCANCODE_PRINTSCREEN = 70,
            SDL_SCANCODE_SCROLLLOCK = 71,
            SDL_SCANCODE_PAUSE = 72,
            SDL_SCANCODE_INSERT = 73, /**< insert on PC, help on some Mac keyboards (but
                                   does send code 73, not 117) */
            SDL_SCANCODE_HOME = 74,
            SDL_SCANCODE_PAGEUP = 75,
            SDL_SCANCODE_DELETE = 76,
            SDL_SCANCODE_END = 77,
            SDL_SCANCODE_PAGEDOWN = 78,
            SDL_SCANCODE_RIGHT = 79,
            SDL_SCANCODE_LEFT = 80,
            SDL_SCANCODE_DOWN = 81,
            SDL_SCANCODE_UP = 82,

            SDL_SCANCODE_NUMLOCKCLEAR = 83, /**< num lock on PC, clear on Mac keyboards
                                     */
            SDL_SCANCODE_KP_DIVIDE = 84,
            SDL_SCANCODE_KP_MULTIPLY = 85,
            SDL_SCANCODE_KP_MINUS = 86,
            SDL_SCANCODE_KP_PLUS = 87,
            SDL_SCANCODE_KP_ENTER = 88,
            SDL_SCANCODE_KP_1 = 89,
            SDL_SCANCODE_KP_2 = 90,
            SDL_SCANCODE_KP_3 = 91,
            SDL_SCANCODE_KP_4 = 92,
            SDL_SCANCODE_KP_5 = 93,
            SDL_SCANCODE_KP_6 = 94,
            SDL_SCANCODE_KP_7 = 95,
            SDL_SCANCODE_KP_8 = 96,
            SDL_SCANCODE_KP_9 = 97,
            SDL_SCANCODE_KP_0 = 98,
            SDL_SCANCODE_KP_PERIOD = 99,

            SDL_SCANCODE_NONUSBACKSLASH = 100, /**< This is the additional key that ISO
                                        *   keyboards have over ANSI ones,
                                        *   located between left shift and Y.
                                        *   Produces GRAVE ACCENT and TILDE in a
                                        *   US or UK Mac layout, REVERSE SOLIDUS
                                        *   (backslash) and VERTICAL LINE in a
                                        *   US or UK Windows layout, and
                                        *   LESS-THAN SIGN and GREATER-THAN SIGN
                                        *   in a Swiss German, German, or French
                                        *   layout. */
            SDL_SCANCODE_APPLICATION = 101, /**< windows contextual menu, compose */
            SDL_SCANCODE_POWER = 102, /**< The USB document says this is a status flag,
                               *   not a physical key - but some Mac keyboards
                               *   do have a power key. */
            SDL_SCANCODE_KP_EQUALS = 103,
            SDL_SCANCODE_F13 = 104,
            SDL_SCANCODE_F14 = 105,
            SDL_SCANCODE_F15 = 106,
            SDL_SCANCODE_F16 = 107,
            SDL_SCANCODE_F17 = 108,
            SDL_SCANCODE_F18 = 109,
            SDL_SCANCODE_F19 = 110,
            SDL_SCANCODE_F20 = 111,
            SDL_SCANCODE_F21 = 112,
            SDL_SCANCODE_F22 = 113,
            SDL_SCANCODE_F23 = 114,
            SDL_SCANCODE_F24 = 115,
            SDL_SCANCODE_EXECUTE = 116,
            SDL_SCANCODE_HELP = 117,    /**< AL Integrated Help Center */
            SDL_SCANCODE_MENU = 118,    /**< Menu (show menu) */
            SDL_SCANCODE_SELECT = 119,
            SDL_SCANCODE_STOP = 120,    /**< AC Stop */
            SDL_SCANCODE_AGAIN = 121,   /**< AC Redo/Repeat */
            SDL_SCANCODE_UNDO = 122,    /**< AC Undo */
            SDL_SCANCODE_CUT = 123,     /**< AC Cut */
            SDL_SCANCODE_COPY = 124,    /**< AC Copy */
            SDL_SCANCODE_PASTE = 125,   /**< AC Paste */
            SDL_SCANCODE_FIND = 126,    /**< AC Find */
            SDL_SCANCODE_MUTE = 127,
            SDL_SCANCODE_VOLUMEUP = 128,
            SDL_SCANCODE_VOLUMEDOWN = 129,
            /* not sure whether there's a reason to enable these */
            /*     SDL_SCANCODE_LOCKINGCAPSLOCK = 130,  */
            /*     SDL_SCANCODE_LOCKINGNUMLOCK = 131, */
            /*     SDL_SCANCODE_LOCKINGSCROLLLOCK = 132, */
            SDL_SCANCODE_KP_COMMA = 133,
            SDL_SCANCODE_KP_EQUALSAS400 = 134,

            SDL_SCANCODE_INTERNATIONAL1 = 135, /**< used on Asian keyboards, see
                                            footnotes in USB doc */
            SDL_SCANCODE_INTERNATIONAL2 = 136,
            SDL_SCANCODE_INTERNATIONAL3 = 137, /**< Yen */
            SDL_SCANCODE_INTERNATIONAL4 = 138,
            SDL_SCANCODE_INTERNATIONAL5 = 139,
            SDL_SCANCODE_INTERNATIONAL6 = 140,
            SDL_SCANCODE_INTERNATIONAL7 = 141,
            SDL_SCANCODE_INTERNATIONAL8 = 142,
            SDL_SCANCODE_INTERNATIONAL9 = 143,
            SDL_SCANCODE_LANG1 = 144, /**< Hangul/English toggle */
            SDL_SCANCODE_LANG2 = 145, /**< Hanja conversion */
            SDL_SCANCODE_LANG3 = 146, /**< Katakana */
            SDL_SCANCODE_LANG4 = 147, /**< Hiragana */
            SDL_SCANCODE_LANG5 = 148, /**< Zenkaku/Hankaku */
            SDL_SCANCODE_LANG6 = 149, /**< reserved */
            SDL_SCANCODE_LANG7 = 150, /**< reserved */
            SDL_SCANCODE_LANG8 = 151, /**< reserved */
            SDL_SCANCODE_LANG9 = 152, /**< reserved */

            SDL_SCANCODE_ALTERASE = 153,    /**< Erase-Eaze */
            SDL_SCANCODE_SYSREQ = 154,
            SDL_SCANCODE_CANCEL = 155,      /**< AC Cancel */
            SDL_SCANCODE_CLEAR = 156,
            SDL_SCANCODE_PRIOR = 157,
            SDL_SCANCODE_RETURN2 = 158,
            SDL_SCANCODE_SEPARATOR = 159,
            SDL_SCANCODE_OUT = 160,
            SDL_SCANCODE_OPER = 161,
            SDL_SCANCODE_CLEARAGAIN = 162,
            SDL_SCANCODE_CRSEL = 163,
            SDL_SCANCODE_EXSEL = 164,

            SDL_SCANCODE_KP_00 = 176,
            SDL_SCANCODE_KP_000 = 177,
            SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
            SDL_SCANCODE_DECIMALSEPARATOR = 179,
            SDL_SCANCODE_CURRENCYUNIT = 180,
            SDL_SCANCODE_CURRENCYSUBUNIT = 181,
            SDL_SCANCODE_KP_LEFTPAREN = 182,
            SDL_SCANCODE_KP_RIGHTPAREN = 183,
            SDL_SCANCODE_KP_LEFTBRACE = 184,
            SDL_SCANCODE_KP_RIGHTBRACE = 185,
            SDL_SCANCODE_KP_TAB = 186,
            SDL_SCANCODE_KP_BACKSPACE = 187,
            SDL_SCANCODE_KP_A = 188,
            SDL_SCANCODE_KP_B = 189,
            SDL_SCANCODE_KP_C = 190,
            SDL_SCANCODE_KP_D = 191,
            SDL_SCANCODE_KP_E = 192,
            SDL_SCANCODE_KP_F = 193,
            SDL_SCANCODE_KP_XOR = 194,
            SDL_SCANCODE_KP_POWER = 195,
            SDL_SCANCODE_KP_PERCENT = 196,
            SDL_SCANCODE_KP_LESS = 197,
            SDL_SCANCODE_KP_GREATER = 198,
            SDL_SCANCODE_KP_AMPERSAND = 199,
            SDL_SCANCODE_KP_DBLAMPERSAND = 200,
            SDL_SCANCODE_KP_VERTICALBAR = 201,
            SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
            SDL_SCANCODE_KP_COLON = 203,
            SDL_SCANCODE_KP_HASH = 204,
            SDL_SCANCODE_KP_SPACE = 205,
            SDL_SCANCODE_KP_AT = 206,
            SDL_SCANCODE_KP_EXCLAM = 207,
            SDL_SCANCODE_KP_MEMSTORE = 208,
            SDL_SCANCODE_KP_MEMRECALL = 209,
            SDL_SCANCODE_KP_MEMCLEAR = 210,
            SDL_SCANCODE_KP_MEMADD = 211,
            SDL_SCANCODE_KP_MEMSUBTRACT = 212,
            SDL_SCANCODE_KP_MEMMULTIPLY = 213,
            SDL_SCANCODE_KP_MEMDIVIDE = 214,
            SDL_SCANCODE_KP_PLUSMINUS = 215,
            SDL_SCANCODE_KP_CLEAR = 216,
            SDL_SCANCODE_KP_CLEARENTRY = 217,
            SDL_SCANCODE_KP_BINARY = 218,
            SDL_SCANCODE_KP_OCTAL = 219,
            SDL_SCANCODE_KP_DECIMAL = 220,
            SDL_SCANCODE_KP_HEXADECIMAL = 221,

            SDL_SCANCODE_LCTRL = 224,
            SDL_SCANCODE_LSHIFT = 225,
            SDL_SCANCODE_LALT = 226, /**< alt, option */
            SDL_SCANCODE_LGUI = 227, /**< windows, command (apple), meta */
            SDL_SCANCODE_RCTRL = 228,
            SDL_SCANCODE_RSHIFT = 229,
            SDL_SCANCODE_RALT = 230, /**< alt gr, option */
            SDL_SCANCODE_RGUI = 231, /**< windows, command (apple), meta */

            SDL_SCANCODE_MODE = 257,    /**< I'm not sure if this is really not covered
                                 *   by any of the above, but since there's a
                                 *   special SDL_KMOD_MODE for it I'm adding it here
                                 */

            /* @} *//* Usage page 0x07 */

            /**
             *  \name Usage page 0x0C
             *
             *  These values are mapped from usage page 0x0C (USB consumer page).
             *
             *  There are way more keys in the spec than we can represent in the
             *  current scancode range, so pick the ones that commonly come up in
             *  real world usage.
             */
            /* @{ */

            SDL_SCANCODE_SLEEP = 258,                   /**< Sleep */
            SDL_SCANCODE_WAKE = 259,                    /**< Wake */

            SDL_SCANCODE_CHANNEL_INCREMENT = 260,       /**< Channel Increment */
            SDL_SCANCODE_CHANNEL_DECREMENT = 261,       /**< Channel Decrement */

            SDL_SCANCODE_MEDIA_PLAY = 262,          /**< Play */
            SDL_SCANCODE_MEDIA_PAUSE = 263,         /**< Pause */
            SDL_SCANCODE_MEDIA_RECORD = 264,        /**< Record */
            SDL_SCANCODE_MEDIA_FAST_FORWARD = 265,  /**< Fast Forward */
            SDL_SCANCODE_MEDIA_REWIND = 266,        /**< Rewind */
            SDL_SCANCODE_MEDIA_NEXT_TRACK = 267,    /**< Next Track */
            SDL_SCANCODE_MEDIA_PREVIOUS_TRACK = 268, /**< Previous Track */
            SDL_SCANCODE_MEDIA_STOP = 269,          /**< Stop */
            SDL_SCANCODE_MEDIA_EJECT = 270,         /**< Eject */
            SDL_SCANCODE_MEDIA_PLAY_PAUSE = 271,    /**< Play / Pause */
            SDL_SCANCODE_MEDIA_SELECT = 272,        /* Media Select */

            SDL_SCANCODE_AC_NEW = 273,              /**< AC New */
            SDL_SCANCODE_AC_OPEN = 274,             /**< AC Open */
            SDL_SCANCODE_AC_CLOSE = 275,            /**< AC Close */
            SDL_SCANCODE_AC_EXIT = 276,             /**< AC Exit */
            SDL_SCANCODE_AC_SAVE = 277,             /**< AC Save */
            SDL_SCANCODE_AC_PRINT = 278,            /**< AC Print */
            SDL_SCANCODE_AC_PROPERTIES = 279,       /**< AC Properties */

            SDL_SCANCODE_AC_SEARCH = 280,           /**< AC Search */
            SDL_SCANCODE_AC_HOME = 281,             /**< AC Home */
            SDL_SCANCODE_AC_BACK = 282,             /**< AC Back */
            SDL_SCANCODE_AC_FORWARD = 283,          /**< AC Forward */
            SDL_SCANCODE_AC_STOP = 284,             /**< AC Stop */
            SDL_SCANCODE_AC_REFRESH = 285,          /**< AC Refresh */
            SDL_SCANCODE_AC_BOOKMARKS = 286,        /**< AC Bookmarks */

            /* @} *//* Usage page 0x0C */


            /**
             *  \name Mobile keys
             *
             *  These are values that are often used on mobile phones.
             */
            /* @{ */

            SDL_SCANCODE_SOFTLEFT = 287, /**< Usually situated below the display on phones and
                                      used as a multi-function feature key for selecting
                                      a software defined function shown on the bottom left
                                      of the display. */
            SDL_SCANCODE_SOFTRIGHT = 288, /**< Usually situated below the display on phones and
                                       used as a multi-function feature key for selecting
                                       a software defined function shown on the bottom right
                                       of the display. */
            SDL_SCANCODE_CALL = 289, /**< Used for accepting phone calls. */
            SDL_SCANCODE_ENDCALL = 290, /**< Used for rejecting phone calls. */

            /* @} *//* Mobile keys */

            /* Add any other keys here. */

            SDL_SCANCODE_RESERVED = 400,    /**< 400-500 reserved for dynamic keycodes */

            SDL_NUM_SCANCODES = 512 /**< not a key, just marks the number of scancodes
                                 for array bounds */
        }
        #endregion

        #region SDL_events.h
        /**
         * The types of events that can be delivered.
         *
         * \since This enum is available since SDL 3.0.0.
         */
        public enum SDL_EventType : UInt32
        {
            SDL_EVENT_FIRST = 0,     /**< Unused (do not remove) */

            /* Application events */
            SDL_EVENT_QUIT = 0x100, /**< User-requested quit */

            /* These application events have special meaning on iOS, see README-ios.md for details */
            SDL_EVENT_TERMINATING,        /**< The application is being terminated by the OS
                                     Called on iOS in applicationWillTerminate()
                                     Called on Android in onDestroy()
                                */
            SDL_EVENT_LOW_MEMORY,          /**< The application is low on memory, free memory if possible.
                                     Called on iOS in applicationDidReceiveMemoryWarning()
                                     Called on Android in onLowMemory()
                                */
            SDL_EVENT_WILL_ENTER_BACKGROUND, /**< The application is about to enter the background
                                     Called on iOS in applicationWillResignActive()
                                     Called on Android in onPause()
                                */
            SDL_EVENT_DID_ENTER_BACKGROUND, /**< The application did enter the background and may not get CPU for some time
                                     Called on iOS in applicationDidEnterBackground()
                                     Called on Android in onPause()
                                */
            SDL_EVENT_WILL_ENTER_FOREGROUND, /**< The application is about to enter the foreground
                                     Called on iOS in applicationWillEnterForeground()
                                     Called on Android in onResume()
                                */
            SDL_EVENT_DID_ENTER_FOREGROUND, /**< The application is now interactive
                                     Called on iOS in applicationDidBecomeActive()
                                     Called on Android in onResume()
                                */

            SDL_EVENT_LOCALE_CHANGED,  /**< The user's locale preferences have changed. */

            SDL_EVENT_SYSTEM_THEME_CHANGED, /**< The system theme changed */

            /* Display events */
            /* 0x150 was SDL_DISPLAYEVENT, reserve the number for sdl2-compat */
            SDL_EVENT_DISPLAY_ORIENTATION = 0x151,   /**< Display orientation has changed to data1 */
            SDL_EVENT_DISPLAY_ADDED,                 /**< Display has been added to the system */
            SDL_EVENT_DISPLAY_REMOVED,               /**< Display has been removed from the system */
            SDL_EVENT_DISPLAY_MOVED,                 /**< Display has changed position */
            SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED, /**< Display has changed content scale */
            SDL_EVENT_DISPLAY_FIRST = SDL_EVENT_DISPLAY_ORIENTATION,
            SDL_EVENT_DISPLAY_LAST = SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED,

            /* Window events */
            /* 0x200 was SDL_WINDOWEVENT, reserve the number for sdl2-compat */
            /* 0x201 was SDL_EVENT_SYSWM, reserve the number for sdl2-compat */
            SDL_EVENT_WINDOW_SHOWN = 0x202,     /**< Window has been shown */
            SDL_EVENT_WINDOW_HIDDEN,            /**< Window has been hidden */
            SDL_EVENT_WINDOW_EXPOSED,           /**< Window has been exposed and should be redrawn, and can be redrawn directly from event watchers for this event */
            SDL_EVENT_WINDOW_MOVED,             /**< Window has been moved to data1, data2 */
            SDL_EVENT_WINDOW_RESIZED,           /**< Window has been resized to data1xdata2 */
            SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED,/**< The pixel size of the window has changed to data1xdata2 */
            SDL_EVENT_WINDOW_MINIMIZED,         /**< Window has been minimized */
            SDL_EVENT_WINDOW_MAXIMIZED,         /**< Window has been maximized */
            SDL_EVENT_WINDOW_RESTORED,          /**< Window has been restored to normal size and position */
            SDL_EVENT_WINDOW_MOUSE_ENTER,       /**< Window has gained mouse focus */
            SDL_EVENT_WINDOW_MOUSE_LEAVE,       /**< Window has lost mouse focus */
            SDL_EVENT_WINDOW_FOCUS_GAINED,      /**< Window has gained keyboard focus */
            SDL_EVENT_WINDOW_FOCUS_LOST,        /**< Window has lost keyboard focus */
            SDL_EVENT_WINDOW_CLOSE_REQUESTED,   /**< The window manager requests that the window be closed */
            SDL_EVENT_WINDOW_HIT_TEST,          /**< Window had a hit test that wasn't SDL_HITTEST_NORMAL */
            SDL_EVENT_WINDOW_ICCPROF_CHANGED,   /**< The ICC profile of the window's display has changed */
            SDL_EVENT_WINDOW_DISPLAY_CHANGED,   /**< Window has been moved to display data1 */
            SDL_EVENT_WINDOW_DISPLAY_SCALE_CHANGED, /**< Window display scale has been changed */
            SDL_EVENT_WINDOW_OCCLUDED,          /**< The window has been occluded */
            SDL_EVENT_WINDOW_ENTER_FULLSCREEN,  /**< The window has entered fullscreen mode */
            SDL_EVENT_WINDOW_LEAVE_FULLSCREEN,  /**< The window has left fullscreen mode */
            SDL_EVENT_WINDOW_DESTROYED,         /**< The window with the associated ID is being or has been destroyed. If this message is being handled
                                             in an event watcher, the window handle is still valid and can still be used to retrieve any userdata
                                             associated with the window. Otherwise, the handle has already been destroyed and all resources
                                             associated with it are invalid */
            SDL_EVENT_WINDOW_PEN_ENTER,         /**< Window has gained focus of the pressure-sensitive pen with ID "data1" */
            SDL_EVENT_WINDOW_PEN_LEAVE,         /**< Window has lost focus of the pressure-sensitive pen with ID "data1" */
            SDL_EVENT_WINDOW_HDR_STATE_CHANGED, /**< Window HDR properties have changed */
            SDL_EVENT_WINDOW_FIRST = SDL_EVENT_WINDOW_SHOWN,
            SDL_EVENT_WINDOW_LAST = SDL_EVENT_WINDOW_PEN_LEAVE,

            /* Keyboard events */
            SDL_EVENT_KEY_DOWN = 0x300, /**< Key pressed */
            SDL_EVENT_KEY_UP,                  /**< Key released */
            SDL_EVENT_TEXT_EDITING,            /**< Keyboard text editing (composition) */
            SDL_EVENT_TEXT_INPUT,              /**< Keyboard text input */
            SDL_EVENT_KEYMAP_CHANGED,          /**< Keymap changed due to a system event such as an
                                            input language or keyboard layout change. */
            SDL_EVENT_KEYBOARD_ADDED,          /**< A new keyboard has been inserted into the system */
            SDL_EVENT_KEYBOARD_REMOVED,        /**< A keyboard has been removed */
            SDL_EVENT_TEXT_EDITING_CANDIDATES, /**< Keyboard text editing candidates */

            /* Mouse events */
            SDL_EVENT_MOUSE_MOTION = 0x400, /**< Mouse moved */
            SDL_EVENT_MOUSE_BUTTON_DOWN,       /**< Mouse button pressed */
            SDL_EVENT_MOUSE_BUTTON_UP,         /**< Mouse button released */
            SDL_EVENT_MOUSE_WHEEL,             /**< Mouse wheel motion */
            SDL_EVENT_MOUSE_ADDED,             /**< A new mouse has been inserted into the system */
            SDL_EVENT_MOUSE_REMOVED,           /**< A mouse has been removed */

            /* Joystick events */
            SDL_EVENT_JOYSTICK_AXIS_MOTION = 0x600, /**< Joystick axis motion */
            SDL_EVENT_JOYSTICK_BALL_MOTION,          /**< Joystick trackball motion */
            SDL_EVENT_JOYSTICK_HAT_MOTION,           /**< Joystick hat position change */
            SDL_EVENT_JOYSTICK_BUTTON_DOWN,          /**< Joystick button pressed */
            SDL_EVENT_JOYSTICK_BUTTON_UP,            /**< Joystick button released */
            SDL_EVENT_JOYSTICK_ADDED,                /**< A new joystick has been inserted into the system */
            SDL_EVENT_JOYSTICK_REMOVED,              /**< An opened joystick has been removed */
            SDL_EVENT_JOYSTICK_BATTERY_UPDATED,      /**< Joystick battery level change */
            SDL_EVENT_JOYSTICK_UPDATE_COMPLETE,      /**< Joystick update is complete */

            /* Gamepad events */
            SDL_EVENT_GAMEPAD_AXIS_MOTION = 0x650, /**< Gamepad axis motion */
            SDL_EVENT_GAMEPAD_BUTTON_DOWN,          /**< Gamepad button pressed */
            SDL_EVENT_GAMEPAD_BUTTON_UP,            /**< Gamepad button released */
            SDL_EVENT_GAMEPAD_ADDED,                /**< A new gamepad has been inserted into the system */
            SDL_EVENT_GAMEPAD_REMOVED,              /**< A gamepad has been removed */
            SDL_EVENT_GAMEPAD_REMAPPED,             /**< The gamepad mapping was updated */
            SDL_EVENT_GAMEPAD_TOUCHPAD_DOWN,        /**< Gamepad touchpad was touched */
            SDL_EVENT_GAMEPAD_TOUCHPAD_MOTION,      /**< Gamepad touchpad finger was moved */
            SDL_EVENT_GAMEPAD_TOUCHPAD_UP,          /**< Gamepad touchpad finger was lifted */
            SDL_EVENT_GAMEPAD_SENSOR_UPDATE,        /**< Gamepad sensor was updated */
            SDL_EVENT_GAMEPAD_UPDATE_COMPLETE,      /**< Gamepad update is complete */
            SDL_EVENT_GAMEPAD_STEAM_HANDLE_UPDATED,  /**< Gamepad Steam handle has changed */

            /* Touch events */
            SDL_EVENT_FINGER_DOWN = 0x700,
            SDL_EVENT_FINGER_UP,
            SDL_EVENT_FINGER_MOTION,

            /* 0x800, 0x801, and 0x802 were the Gesture events from SDL2. Do not reuse these values! sdl2-compat needs them! */

            /* Clipboard events */
            SDL_EVENT_CLIPBOARD_UPDATE = 0x900, /**< The clipboard or primary selection changed */

            /* Drag and drop events */
            SDL_EVENT_DROP_FILE = 0x1000, /**< The system requests a file open */
            SDL_EVENT_DROP_TEXT,                 /**< text/plain drag-and-drop event */
            SDL_EVENT_DROP_BEGIN,                /**< A new set of drops is beginning (NULL filename) */
            SDL_EVENT_DROP_COMPLETE,             /**< Current set of drops is now complete (NULL filename) */
            SDL_EVENT_DROP_POSITION,             /**< Position while moving over the window */

            /* Audio hotplug events */
            SDL_EVENT_AUDIO_DEVICE_ADDED = 0x1100,  /**< A new audio device is available */
            SDL_EVENT_AUDIO_DEVICE_REMOVED,         /**< An audio device has been removed. */
            SDL_EVENT_AUDIO_DEVICE_FORMAT_CHANGED,  /**< An audio device's format has been changed by the system. */

            /* Sensor events */
            SDL_EVENT_SENSOR_UPDATE = 0x1200,     /**< A sensor was updated */

            /* Pressure-sensitive pen events */
            SDL_EVENT_PEN_DOWN = 0x1300,     /**< Pressure-sensitive pen touched drawing surface */
            SDL_EVENT_PEN_UP,                     /**< Pressure-sensitive pen stopped touching drawing surface */
            SDL_EVENT_PEN_MOTION,                 /**< Pressure-sensitive pen moved, or angle/pressure changed */
            SDL_EVENT_PEN_BUTTON_DOWN,            /**< Pressure-sensitive pen button pressed */
            SDL_EVENT_PEN_BUTTON_UP,              /**< Pressure-sensitive pen button released */

            /* Camera hotplug events */
            SDL_EVENT_CAMERA_DEVICE_ADDED = 0x1400,  /**< A new camera device is available */
            SDL_EVENT_CAMERA_DEVICE_REMOVED,         /**< A camera device has been removed. */
            SDL_EVENT_CAMERA_DEVICE_APPROVED,        /**< A camera device has been approved for use by the user. */
            SDL_EVENT_CAMERA_DEVICE_DENIED,          /**< A camera device has been denied for use by the user. */

            /* Render events */
            SDL_EVENT_RENDER_TARGETS_RESET = 0x2000, /**< The render targets have been reset and their contents need to be updated */
            SDL_EVENT_RENDER_DEVICE_RESET, /**< The device has been reset and all textures need to be recreated */

            /* Internal events */
            SDL_EVENT_POLL_SENTINEL = 0x7F00, /**< Signals the end of an event poll cycle */

            /** Events SDL_EVENT_USER through SDL_EVENT_LAST are for your use,
             *  and should be allocated with SDL_RegisterEvents()
             */
            SDL_EVENT_USER = 0x8000,

            /**
             *  This last event is only for bounding internal arrays
             */
            SDL_EVENT_LAST = 0xFFFF,
        }

        /**
         * Fields shared by every event
         *
         * \since This struct is available since SDL 3.0.0.
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_CommonEvent
        {
            public UInt32 type;        /**< Event type, shared with all events, UInt32 to cover user events which are not in the SDL_EventType enumeration */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
        }

        /**
         * Display state change event data (event.display.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_DisplayEvent
        {
            SDL_EventType type; /**< SDL_DISPLAYEVENT_* */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 displayID;/**< The associated display */
            public Int32 data1;       /**< event dependent data */
        }

        /**
         * Window state change event data (event.window.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_WindowEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_WINDOW_* */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 windowID; /**< The associated window */
            public Int32 data1;       /**< event dependent data */
            public Int32 data2;       /**< event dependent data */
        }

        /**
         * Keyboard device event structure (event.kdevice.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_KeyboardDeviceEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_KEYBOARD_ADDED or SDL_EVENT_KEYBOARD_REMOVED */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 which;   /**< The keyboard instance id */
        }

        /**
         * Keyboard button event structure (event.key.*)
         *
         * The `key` is the base SDL_Keycode generated by pressing the `scancode`
         * using the current keyboard layout, applying any options specified in
         * SDL_HINT_KEYCODE_OPTIONS. You can get the SDL_Keycode corresponding to the
         * event scancode and modifiers directly from the keyboard layout, bypassing
         * SDL_HINT_KEYCODE_OPTIONS, by calling SDL_GetKeyFromScancode().
         *
         * \since This struct is available since SDL 3.0.0.
         *
         * \sa SDL_GetKeyFromScancode
         * \sa SDL_HINT_KEYCODE_OPTIONS
         */
        public struct SDL_KeyboardEvent
        {
            public SDL_EventType type;     /**< SDL_EVENT_KEY_DOWN or SDL_EVENT_KEY_UP */
            public UInt32 reserved;
            public UInt64 timestamp;       /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 windowID;  /**< The window with keyboard focus, if any */
            public UInt32 which;   /**< The keyboard instance id, or 0 if unknown or virtual */
            public SDL_Scancode scancode;  /**< SDL physical key code */
            public UInt32 key;        /**< SDL virtual key code */
            public UInt16 mod;         /**< current key modifiers */
            public UInt16 raw;             /**< The platform dependent scancode for this event */
            public byte state;            /**< SDL_PRESSED or SDL_RELEASED */
            public byte repeat;           /**< Non-zero if this is a key repeat */
        }

        /**
         * The list of buttons available on a gamepad
         *
         * For controllers that use a diamond pattern for the face buttons, the
         * south/east/west/north buttons below correspond to the locations in the
         * diamond pattern. For Xbox controllers, this would be A/B/X/Y, for Nintendo
         * Switch controllers, this would be B/A/Y/X, for PlayStation controllers this
         * would be Cross/Circle/Square/Triangle.
         *
         * For controllers that don't use a diamond pattern for the face buttons, the
         * south/east/west/north buttons indicate the buttons labeled A, B, C, D, or
         * 1, 2, 3, 4, or for controllers that aren't labeled, they are the primary,
         * secondary, etc. buttons.
         *
         * The activate action is often the south button and the cancel action is
         * often the east button, but in some regions this is reversed, so your game
         * should allow remapping actions based on user preferences.
         *
         * You can query the labels for the face buttons using
         * SDL_GetGamepadButtonLabel()
         *
         * \since This enum is available since SDL 3.0.0.
         */
        public enum SDL_GamepadButton : byte
        {
            SDL_GAMEPAD_BUTTON_SOUTH = 0,           /* Bottom face button (e.g. Xbox A button) */
            SDL_GAMEPAD_BUTTON_EAST,            /* Right face button (e.g. Xbox B button) */
            SDL_GAMEPAD_BUTTON_WEST,            /* Left face button (e.g. Xbox X button) */
            SDL_GAMEPAD_BUTTON_NORTH,           /* Top face button (e.g. Xbox Y button) */
            SDL_GAMEPAD_BUTTON_BACK,
            SDL_GAMEPAD_BUTTON_GUIDE,
            SDL_GAMEPAD_BUTTON_START,
            SDL_GAMEPAD_BUTTON_LEFT_STICK,
            SDL_GAMEPAD_BUTTON_RIGHT_STICK,
            SDL_GAMEPAD_BUTTON_LEFT_SHOULDER,
            SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER,
            SDL_GAMEPAD_BUTTON_DPAD_UP,
            SDL_GAMEPAD_BUTTON_DPAD_DOWN,
            SDL_GAMEPAD_BUTTON_DPAD_LEFT,
            SDL_GAMEPAD_BUTTON_DPAD_RIGHT,
            SDL_GAMEPAD_BUTTON_MISC1,           /* Additional button (e.g. Xbox Series X share button, PS5 microphone button, Nintendo Switch Pro capture button, Amazon Luna microphone button, Google Stadia capture button) */
            SDL_GAMEPAD_BUTTON_RIGHT_PADDLE1,   /* Upper or primary paddle, under your right hand (e.g. Xbox Elite paddle P1) */
            SDL_GAMEPAD_BUTTON_LEFT_PADDLE1,    /* Upper or primary paddle, under your left hand (e.g. Xbox Elite paddle P3) */
            SDL_GAMEPAD_BUTTON_RIGHT_PADDLE2,   /* Lower or secondary paddle, under your right hand (e.g. Xbox Elite paddle P2) */
            SDL_GAMEPAD_BUTTON_LEFT_PADDLE2,    /* Lower or secondary paddle, under your left hand (e.g. Xbox Elite paddle P4) */
            SDL_GAMEPAD_BUTTON_TOUCHPAD,        /* PS4/PS5 touchpad button */
            SDL_GAMEPAD_BUTTON_MISC2,           /* Additional button */
            SDL_GAMEPAD_BUTTON_MISC3,           /* Additional button */
            SDL_GAMEPAD_BUTTON_MISC4,           /* Additional button */
            SDL_GAMEPAD_BUTTON_MISC5,           /* Additional button */
            SDL_GAMEPAD_BUTTON_MISC6,           /* Additional button */
            SDL_GAMEPAD_BUTTON_MAX,
            SDL_GAMEPAD_BUTTON_INVALID = 255
        }

        /**
         * The list of axes available on a gamepad
         *
         * Thumbstick axis values range from SDL_JOYSTICK_AXIS_MIN to
         * SDL_JOYSTICK_AXIS_MAX, and are centered within ~8000 of zero, though
         * advanced UI will allow users to set or autodetect the dead zone, which
         * varies between gamepads.
         *
         * Trigger axis values range from 0 (released) to SDL_JOYSTICK_AXIS_MAX (fully
         * pressed) when reported by SDL_GetGamepadAxis(). Note that this is not the
         * same range that will be reported by the lower-level SDL_GetJoystickAxis().
         *
         * \since This enum is available since SDL 3.0.0.
         */
        public enum SDL_GamepadAxis : byte
        {
            SDL_GAMEPAD_AXIS_LEFTX = 0,
            SDL_GAMEPAD_AXIS_LEFTY,
            SDL_GAMEPAD_AXIS_RIGHTX,
            SDL_GAMEPAD_AXIS_RIGHTY,
            SDL_GAMEPAD_AXIS_LEFT_TRIGGER,
            SDL_GAMEPAD_AXIS_RIGHT_TRIGGER,
            SDL_GAMEPAD_AXIS_MAX,
            SDL_GAMEPAD_AXIS_INVALID = 255,
        }

        /**
         * Gamepad axis motion event structure (event.gaxis.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_GamepadAxisEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_GAMEPAD_AXIS_MOTION */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 /*SDL_JoystickID*/ which; /**< The joystick instance id */
            public SDL_GamepadAxis axis;         /**< The gamepad axis (SDL_GamepadAxis) */
            public byte padding1;
            public byte padding2;
            public byte padding3;
            public Int16 value;       /**< The axis value (range: -32768 to 32767) */
            public UInt16 padding4;
        }

        /**
         * Gamepad button event structure (event.gbutton.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_GamepadButtonEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_GAMEPAD_BUTTON_DOWN or SDL_EVENT_GAMEPAD_BUTTON_UP */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 /*SDL_JoystickID*/ which; /**< The joystick instance id */
            public SDL_GamepadButton button;       /**< The gamepad button (SDL_GamepadButton) */
            public byte state;        /**< SDL_PRESSED or SDL_RELEASED */
            public byte padding1;
            public byte padding2;
        }


        /**
         * Gamepad device event structure (event.gdevice.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_GamepadDeviceEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_GAMEPAD_ADDED, SDL_EVENT_GAMEPAD_REMOVED, or SDL_EVENT_GAMEPAD_REMAPPED, SDL_EVENT_GAMEPAD_UPDATE_COMPLETE or SDL_EVENT_GAMEPAD_STEAM_HANDLE_UPDATED */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 /*SDL_JoystickID*/ which;       /**< The joystick instance id */
        }

        /**
         * Gamepad touchpad event structure (event.gtouchpad.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_GamepadTouchpadEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_GAMEPAD_TOUCHPAD_DOWN or SDL_EVENT_GAMEPAD_TOUCHPAD_MOTION or SDL_EVENT_GAMEPAD_TOUCHPAD_UP */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 /*SDL_JoystickID*/ which; /**< The joystick instance id */
            public Int32 touchpad;    /**< The index of the touchpad */
            public Int32 finger;      /**< The index of the finger on the touchpad */
            public float x;            /**< Normalized in the range 0...1 with 0 being on the left */
            public float y;            /**< Normalized in the range 0...1 with 0 being at the top */
            public float pressure;     /**< Normalized in the range 0...1 */
        }

        /**
         * Gamepad sensor event structure (event.gsensor.*)
         *
         * \since This struct is available since SDL 3.0.0.
         */
        public struct SDL_GamepadSensorEvent
        {
            public SDL_EventType type; /**< SDL_EVENT_GAMEPAD_SENSOR_UPDATE */
            public UInt32 reserved;
            public UInt64 timestamp;   /**< In nanoseconds, populated using SDL_GetTicksNS() */
            public UInt32 /*SDL_JoystickID*/ which; /**< The joystick instance id */
            public Int32 sensor;      /**< The type of the sensor, one of the values of SDL_SensorType */
            public unsafe fixed float data[3];      /**< Up to 3 values from the sensor, as defined in SDL_sensor.h */
            public UInt64 sensor_timestamp; /**< The timestamp of the sensor reading in nanoseconds, not necessarily synchronized with the system clock */
        }

        /**
         * The structure for all events in SDL.
         *
         * \since This struct is available since SDL 3.0.0.
         */
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct SDL_Event
        {
            [FieldOffset(0)]
            public SDL_EventType type;                            /**< Event type, shared with all events, UInt32 to cover user events which are not in the SDL_EventType enumeration */

            [FieldOffset(0)]
            public UInt32 type_uint;                            /**< Event type, shared with all events, UInt32 to cover user events which are not in the SDL_EventType enumeration */

            [FieldOffset(0)]
            public SDL_CommonEvent common;                 /**< Common event data */

            [FieldOffset(0)]
            public SDL_DisplayEvent display;               /**< Display event data */

            [FieldOffset(0)]
            public SDL_WindowEvent window;                 /**< Window event data */

            [FieldOffset(0)]
            public SDL_KeyboardDeviceEvent kdevice;        /**< Keyboard device change event data */

            [FieldOffset(0)]
            public SDL_KeyboardEvent key;                  /**< Keyboard event data */

#if false
            SDL_TextEditingEvent edit;              /**< Text editing event data */
            SDL_TextEditingCandidatesEvent edit_candidates; /**< Text editing candidates event data */
            SDL_TextInputEvent text;                /**< Text input event data */
#endif

#if false
            SDL_MouseDeviceEvent mdevice;           /**< Mouse device change event data */
            SDL_MouseMotionEvent motion;            /**< Mouse motion event data */
            SDL_MouseButtonEvent button;            /**< Mouse button event data */
            SDL_MouseWheelEvent wheel;              /**< Mouse wheel event data */
            SDL_JoyDeviceEvent jdevice;             /**< Joystick device change event data */
            SDL_JoyAxisEvent jaxis;                 /**< Joystick axis event data */
            SDL_JoyBallEvent jball;                 /**< Joystick ball event data */
            SDL_JoyHatEvent jhat;                   /**< Joystick hat event data */
            SDL_JoyButtonEvent jbutton;             /**< Joystick button event data */
            SDL_JoyBatteryEvent jbattery;           /**< Joystick battery event data */
#endif
            [FieldOffset(0)]
            public SDL_GamepadDeviceEvent gdevice;         /**< Gamepad device event data */

            [FieldOffset(0)]
            public SDL_GamepadAxisEvent gaxis;             /**< Gamepad axis event data */

            [FieldOffset(0)]
            public SDL_GamepadButtonEvent gbutton;         /**< Gamepad button event data */

            [FieldOffset(0)]
            public SDL_GamepadTouchpadEvent gtouchpad;     /**< Gamepad touchpad event data */

            [FieldOffset(0)]
            public SDL_GamepadSensorEvent gsensor;         /**< Gamepad sensor event data */
#if false
            SDL_AudioDeviceEvent adevice;           /**< Audio device event data */
            SDL_CameraDeviceEvent cdevice;          /**< Camera device event data */
            SDL_SensorEvent sensor;                 /**< Sensor event data */
            SDL_QuitEvent quit;                     /**< Quit request event data */
            SDL_UserEvent user;                     /**< Custom event data */
            SDL_TouchFingerEvent tfinger;           /**< Touch finger event data */
            SDL_PenTipEvent ptip;                   /**< Pen tip touching or leaving drawing surface */
            SDL_PenMotionEvent pmotion;             /**< Pen change in position, pressure, or angle */
            SDL_PenButtonEvent pbutton;             /**< Pen button press */
            SDL_DropEvent drop;                     /**< Drag and drop event data */
            SDL_ClipboardEvent clipboard;           /**< Clipboard event data */
#endif

            /* This is necessary for ABI compatibility between Visual C++ and GCC.
               Visual C++ will respect the push pack pragma and use 52 bytes (size of
               SDL_TextEditingEvent, the largest structure for 32-bit and 64-bit
               architectures) for this union, and GCC will use the alignment of the
               largest datatype within the union, which is 8 bytes on 64-bit
               architectures.

               So... we'll add padding to force the size to be the same for both.

               On architectures where pointers are 16 bytes, this needs rounding up to
               the next multiple of 16, 64, and on architectures where pointers are
               even larger the size of SDL_UserEvent will dominate as being 3 pointers.
            */
            [FieldOffset(0)]
            private fixed byte padding[128];
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_PollEvent(out SDL_Event _event);
        #endregion

        #region SDL_error
        /**
         * Retrieve a message about the last error that occurred on the current
         * thread.
         *
         * It is possible for multiple errors to occur before calling SDL_GetError().
         * Only the last error is returned.
         *
         * The message is only applicable when an SDL function has signaled an error.
         * You must check the return values of SDL function calls to determine when to
         * appropriately call SDL_GetError(). You should *not* use the results of
         * SDL_GetError() to decide if an error has occurred! Sometimes SDL will set
         * an error string even when reporting success.
         *
         * SDL will *not* clear the error string for successful API calls. You *must*
         * check return values for failure cases before you can assume the error
         * string applies.
         *
         * Error strings are set per-thread, so an error set in a different thread
         * will not interfere with the current thread's operation.
         *
         * The returned string does **NOT** follow the SDL_GetStringRule! The pointer
         * is valid until the current thread's error string is changed, so the caller
         * should make a copy if the string is to be used after calling into SDL
         * again.
         *
         * \returns a message with information about the specific error that occurred,
         *          or an empty string if there hasn't been an error message set since
         *          the last call to SDL_ClearError().
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_ClearError
         * \sa SDL_SetError
         */
        [DllImport(nativeLibName, EntryPoint="SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe char* Inner_SDL_GetError();

        public static string SDL_GetError()
        {
            unsafe
            {
                string? Error = Marshal.PtrToStringAnsi((IntPtr)Inner_SDL_GetError());
                if (Error != null)
                {
                    return Error;
                }
            }
            return "";
        }
        #endregion

        #region SDL_joystick
        /**
         * Get a list of currently connected joysticks.
         *
         * \param count a pointer filled in with the number of joysticks returned.
         * \returns a 0 terminated array of joystick instance IDs which should be
         *          freed with SDL_free(), or NULL on error; call SDL_GetError() for
         *          more details.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_HasJoystick
         * \sa SDL_OpenJoystick
         */
        [DllImport(nativeLibName, EntryPoint="SDL_GetJoysticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe UInt32* /*SDL_JoystickID* */ Inner_SDL_GetJoysticks(int *count);

        public static UInt32[] SDL_GetJoysticks()
        {
            unsafe
            {
                int Count = 0;
                UInt32* Found = Inner_SDL_GetJoysticks(&Count);
                var Result = new UInt32[Count];
                for (int i = 0; i < Count; ++i)
                {
                    Result[i] = Found[i];
                }
                return Result;
            }

        }
        #endregion

        #region SDL_gamepad
        /**
         * Open a gamepad for use.
         *
         * \param instance_id the joystick instance ID.
         * \returns a gamepad identifier or NULL if an error occurred; call
         *          SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CloseGamepad
         * \sa SDL_IsGamepad
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_Gamepad_Ptr SDL_OpenGamepad(UInt32 /*SDL_JoystickID*/ instance_id);
        #endregion

        #region SDL_iostream
        /**
         * Use this function to prepare a read-write memory buffer for use with
         * SDL_IOStream.
         *
         * This function sets up an SDL_IOStream struct based on a memory area of a
         * certain size, for both read and write access.
         *
         * This memory buffer is not copied by the SDL_IOStream; the pointer you
         * provide must remain valid until you close the stream. Closing the stream
         * will not free the original buffer.
         *
         * If you need to make sure the SDL_IOStream never writes to the memory
         * buffer, you should use SDL_IOFromConstMem() with a read-only buffer of
         * memory instead.
         *
         * \param mem a pointer to a buffer to feed an SDL_IOStream stream.
         * \param size the buffer size, in bytes.
         * \returns a pointer to a new SDL_IOStream structure, or NULL if it fails;
         *          call SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_IOFromConstMem
         * \sa SDL_CloseIO
         * \sa SDL_ReadIO
         * \sa SDL_SeekIO
         * \sa SDL_TellIO
         * \sa SDL_WriteIO
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_IOStream_Ptr SDL_IOFromMem(void* mem, size_t size);
        #endregion

        #region SDL_surface
        /**
         * Allocate a new RGB surface with a specific pixel format and existing pixel
         * data.
         *
         * No copy is made of the pixel data. Pixel data is not managed automatically;
         * you must free the surface before you free the pixel data.
         *
         * Pitch is the offset in bytes from one row of pixels to the next, e.g.
         * `width*4` for `SDL_PIXELFORMAT_RGBA8888`.
         *
         * You may pass NULL for pixels and 0 for pitch to create a surface that you
         * will fill in with valid values later.
         *
         * \param pixels a pointer to existing pixel data.
         * \param width the width of the surface.
         * \param height the height of the surface.
         * \param pitch the number of bytes between each row, including padding.
         * \param format the SDL_PixelFormatEnum for the new surface's pixel format.
         * \returns the new SDL_Surface structure that is created or NULL if it fails;
         *          call SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CreateSurface
         * \sa SDL_DestroySurface
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_Surface_Ptr SDL_CreateSurfaceFrom(
            void* pixels, int width, int height, int pitch, UInt32 /*SDL_PixelFormatEnum*/ format);

        /**
         * Free an RGB surface.
         *
         * It is safe to pass NULL to this function.
         *
         * \param surface the SDL_Surface to free.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CreateSurface
         * \sa SDL_CreateSurfaceFrom
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_DestroySurface(SDL_Surface_Ptr surface);

        /**
         * Load a BMP image from a seekable SDL data stream.
         *
         * The new surface should be freed with SDL_DestroySurface(). Not doing so
         * will result in a memory leak.
         *
         * \param src the data stream for the surface.
         * \param closeio if SDL_TRUE, calls SDL_CloseIO() on `src` before returning,
         *                even in the case of an error.
         * \returns a pointer to a new SDL_Surface structure or NULL if there was an
         *          error; call SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_DestroySurface
         * \sa SDL_LoadBMP
         * \sa SDL_SaveBMP_IO
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_Surface_Ptr SDL_LoadBMP_IO(SDL_IOStream_Ptr stream, SDL_bool closeio);
        #endregion

        #region SDL_video
        public const UInt64 SDL_WINDOW_FULLSCREEN           = 0x0000000000000001;    /**< window is in fullscreen mode */
        public const UInt64 SDL_WINDOW_OPENGL               = 0x0000000000000002;    /**< window usable with OpenGL context */
        public const UInt64 SDL_WINDOW_OCCLUDED             = 0x0000000000000004;    /**< window is occluded */
        public const UInt64 SDL_WINDOW_HIDDEN               = 0x0000000000000008;    /**< window is neither mapped onto the desktop nor shown in the taskbar/dock/window list; SDL_ShowWindow() is required for it to become visible */
        public const UInt64 SDL_WINDOW_BORDERLESS           = 0x0000000000000010;    /**< no window decoration */
        public const UInt64 SDL_WINDOW_RESIZABLE            = 0x0000000000000020;    /**< window can be resized */
        public const UInt64 SDL_WINDOW_MINIMIZED            = 0x0000000000000040;    /**< window is minimized */
        public const UInt64 SDL_WINDOW_MAXIMIZED            = 0x0000000000000080;    /**< window is maximized */
        public const UInt64 SDL_WINDOW_MOUSE_GRABBED        = 0x0000000000000100;    /**< window has grabbed mouse input */
        public const UInt64 SDL_WINDOW_INPUT_FOCUS          = 0x0000000000000200;    /**< window has input focus */
        public const UInt64 SDL_WINDOW_MOUSE_FOCUS          = 0x0000000000000400;    /**< window has mouse focus */
        public const UInt64 SDL_WINDOW_EXTERNAL             = 0x0000000000000800;    /**< window not created by SDL */
        public const UInt64 SDL_WINDOW_MODAL                = 0x0000000000001000;    /**< window is modal */
        public const UInt64 SDL_WINDOW_HIGH_PIXEL_DENSITY   = 0x0000000000002000;    /**< window uses high pixel density back buffer if possible */
        public const UInt64 SDL_WINDOW_MOUSE_CAPTURE        = 0x0000000000004000;    /**< window has mouse captured (unrelated to MOUSE_GRABBED) */
        public const UInt64 SDL_WINDOW_ALWAYS_ON_TOP        = 0x0000000000008000;    /**< window should always be above others */
        public const UInt64 SDL_WINDOW_UTILITY              = 0x0000000000020000;    /**< window should be treated as a utility window, not showing in the task bar and window list */
        public const UInt64 SDL_WINDOW_TOOLTIP              = 0x0000000000040000;    /**< window should be treated as a tooltip and does not get mouse or keyboard focus, requires a parent window */
        public const UInt64 SDL_WINDOW_POPUP_MENU           = 0x0000000000080000;    /**< window should be treated as a popup menu, requires a parent window */
        public const UInt64 SDL_WINDOW_KEYBOARD_GRABBED     = 0x0000000000100000;    /**< window has grabbed keyboard input */
        public const UInt64 SDL_WINDOW_VULKAN               = 0x0000000010000000;   /**< window usable for Vulkan surface */
        public const UInt64 SDL_WINDOW_METAL                = 0x0000000020000000;    /**< window usable for Metal view */
        public const UInt64 SDL_WINDOW_TRANSPARENT          = 0x0000000040000000;    /**< window with transparent buffer */
        public const UInt64 SDL_WINDOW_NOT_FOCUSABLE        = 0x0000000080000000;   /**< window should not be focusable */

        public struct SDL_DisplayMode
        {
            public UInt32 displayID;    /**< the display this mode is associated with */
            public UInt32 /*SDL_PixelFormatEnum*/ format; /**< pixel format */
            public Int32 w;                      /**< width */
            public Int32 h;                      /**< height */
            public float pixel_density;        /**< scale converting size to pixels (e.g. a 1920x1080 mode with 2.0 scale would have 3840x2160 pixels) */
            public float refresh_rate;         /**< refresh rate (or zero for unspecified) */
            public IntPtr driverdata;           /**< driver-specific data, initialize to 0 */
        }

        /**
         * Get the pixel density of a window.
         *
         * This is a ratio of pixel size to window size. For example, if the window is
         * 1920x1080 and it has a high density back buffer of 3840x2160 pixels, it
         * would have a pixel density of 2.0.
         *
         * \param window the window to query.
         * \returns the pixel density or 0.0f on failure; call SDL_GetError() for more
         *          information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_GetWindowDisplayScale
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float SDL_GetWindowPixelDensity(SDL_Window_Ptr window);

        /**
         * Get the content display scale relative to a window's pixel size.
         *
         * This is a combination of the window pixel density and the display content
         * scale, and is the expected scale for displaying content in this window. For
         * example, if a 3840x2160 window had a display scale of 2.0, the user expects
         * the content to take twice as many pixels and be the same physical size as
         * if it were being displayed in a 1920x1080 window with a display scale of
         * 1.0.
         *
         * Conceptually this value corresponds to the scale display setting, and is
         * updated when that setting is changed, or the window moves to a display with
         * a different scale setting.
         *
         * \param window the window to query.
         * \returns the display scale, or 0.0f on failure; call SDL_GetError() for
         *          more information.
         *
         * \since This function is available since SDL 3.0.0.
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float SDL_GetWindowDisplayScale(SDL_Window_Ptr window);

        /**
         * Query the display mode to use when a window is visible at fullscreen.
         *
         * \param window the window to query.
         * \returns a pointer to the exclusive fullscreen mode to use or NULL for
         *          borderless fullscreen desktop mode.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_SetWindowFullscreenMode
         * \sa SDL_SetWindowFullscreen
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_DisplayMode SDL_GetWindowFullscreenMode(SDL_Window_Ptr window);

        /**
         * Create a window with the specified dimensions and flags.
         *
         * `flags` may be any of the following OR'd together:
         *
         * - `SDL_WINDOW_FULLSCREEN`: fullscreen window at desktop resolution
         * - `SDL_WINDOW_OPENGL`: window usable with an OpenGL context
         * - `SDL_WINDOW_VULKAN`: window usable with a Vulkan instance
         * - `SDL_WINDOW_METAL`: window usable with a Metal instance
         * - `SDL_WINDOW_HIDDEN`: window is not visible
         * - `SDL_WINDOW_BORDERLESS`: no window decoration
         * - `SDL_WINDOW_RESIZABLE`: window can be resized
         * - `SDL_WINDOW_MINIMIZED`: window is minimized
         * - `SDL_WINDOW_MAXIMIZED`: window is maximized
         * - `SDL_WINDOW_MOUSE_GRABBED`: window has grabbed mouse focus
         *
         * The SDL_Window is implicitly shown if SDL_WINDOW_HIDDEN is not set.
         *
         * On Apple's macOS, you **must** set the NSHighResolutionCapable Info.plist
         * property to YES, otherwise you will not receive a High-DPI OpenGL canvas.
         *
         * The window pixel size may differ from its window coordinate size if the
         * window is on a high pixel density display. Use SDL_GetWindowSize() to query
         * the client area's size in window coordinates, and
         * SDL_GetWindowSizeInPixels() or SDL_GetRenderOutputSize() to query the
         * drawable size in pixels. Note that the drawable size can vary after the
         * window is created and should be queried again if you get an
         * SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED event.
         *
         * If the window is created with any of the SDL_WINDOW_OPENGL or
         * SDL_WINDOW_VULKAN flags, then the corresponding LoadLibrary function
         * (SDL_GL_LoadLibrary or SDL_Vulkan_LoadLibrary) is called and the
         * corresponding UnloadLibrary function is called by SDL_DestroyWindow().
         *
         * If SDL_WINDOW_VULKAN is specified and there isn't a working Vulkan driver,
         * SDL_CreateWindow() will fail because SDL_Vulkan_LoadLibrary() will fail.
         *
         * If SDL_WINDOW_METAL is specified on an OS that does not support Metal,
         * SDL_CreateWindow() will fail.
         *
         * If you intend to use this window with an SDL_Renderer, you should use
         * SDL_CreateWindowAndRenderer() instead of this function, to avoid window
         * flicker.
         *
         * On non-Apple devices, SDL requires you to either not link to the Vulkan
         * loader or link to a dynamic library version. This limitation may be removed
         * in a future version of SDL.
         *
         * \param title the title of the window, in UTF-8 encoding.
         * \param w the width of the window.
         * \param h the height of the window.
         * \param flags 0, or one or more SDL_WindowFlags OR'd together.
         * \returns the window that was created or NULL on failure; call
         *          SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CreatePopupWindow
         * \sa SDL_CreateWindowWithProperties
         * \sa SDL_DestroyWindow
         */
        [DllImport(nativeLibName, EntryPoint = "SDL_CreateWindow", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe SDL_Window_Ptr Inner_SDL_CreateWindow(byte* title, int w, int h, UInt64 flags);

        // Pass in title strings like so: SDL_CreateWindow("My Game Title"u8, etc, etc)
        public static SDL_Window_Ptr SDL_CreateWindow(ReadOnlySpan<byte> title, int w, int h, UInt64 flags)
        {
            unsafe
            {
                if (title == null)
                {
                    byte* NullPtr = (byte*)0;
                    return Inner_SDL_CreateWindow(NullPtr, w, h, flags);
                }
                else
                {
                    fixed (byte* titlePtr = title)
                    {
                        return Inner_SDL_CreateWindow(titlePtr, w, h, flags);
                    }
                }
            }
        }

        /**
         * Destroy a window.
         *
         * Any popups or modal windows owned by the window will be recursively
         * destroyed as well.
         *
         * If `window` is NULL, this function will return immediately after setting
         * the SDL error message to "Invalid window". See SDL_GetError().
         *
         * \param window the window to destroy.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CreatePopupWindow
         * \sa SDL_CreateWindow
         * \sa SDL_CreateWindowWithProperties
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_DestroyWindow(SDL_Window_Ptr window);

        /**
         * Set the icon for a window.
         *
         * \param window the window to change.
         * \param icon an SDL_Surface structure containing the icon for the window.
         * \returns 0 on success or a negative error code on failure; call
         *          SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetWindowIcon(SDL_Window_Ptr window, SDL_Surface_Ptr icon);

        /**
         * Get the size of a window's client area.
         *
         * The window pixel size may differ from its window coordinate size if the
         * window is on a high pixel density display. Use SDL_GetWindowSizeInPixels()
         * or SDL_GetRenderOutputSize() to get the real client area size in pixels.
         *
         * \param window the window to query the width and height from.
         * \param w a pointer filled in with the width of the window, may be NULL.
         * \param h a pointer filled in with the height of the window, may be NULL.
         * \returns 0 on success or a negative error code on failure; call
         *          SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_GetRenderOutputSize
         * \sa SDL_GetWindowSizeInPixels
         * \sa SDL_SetWindowSize
         */
        [DllImport(nativeLibName, EntryPoint = "SDL_GetWindowSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int Inner_SDL_GetWindowSize(SDL_Window_Ptr window, int* w, int* h);
        public static (int Width, int Height) SDL_GetWindowSize(SDL_Window_Ptr window)
        {
            unsafe
            {
                int w = 0;
                int h = 0;
                int result = Inner_SDL_GetWindowSize(window, &w, &h);
                if (result == 0)
                {
                    return (w, h);
                }
                else
                {
                    return (0, 0);
                }
            }
        }

        /**
         * Get the size of a window's client area, in pixels.
         *
         * \param window the window from which the drawable size should be queried.
         * \param w a pointer to variable for storing the width in pixels, may be
         *          NULL.
         * \param h a pointer to variable for storing the height in pixels, may be
         *          NULL.
         * \returns 0 on success or a negative error code on failure; call
         *          SDL_GetError() for more information.
         *
         * \since This function is available since SDL 3.0.0.
         *
         * \sa SDL_CreateWindow
         * \sa SDL_GetWindowSize
         */
        [DllImport(nativeLibName, EntryPoint = "SDL_GetWindowSizeInPixels", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int Inner_SDL_GetWindowSizeInPixels(SDL_Window_Ptr window, int* w, int* h);
        public static (int Width, int Height) SDL_GetWindowSizeInPixels(SDL_Window_Ptr window)
        {
            unsafe
            {
                int w = 0;
                int h = 0;
                int result = Inner_SDL_GetWindowSizeInPixels(window, &w, &h);
                if (result == 0)
                {
                    return (w, h);
                }
                else
                {
                    return (0, 0);
                }
            }
        }
        #endregion

        #region SDL_gpu.h
        /*
          Simple DirectMedia Layer
          Copyright (C) 1997-2024 Sam Lantinga <slouken@libsdl.org>

          This software is provided 'as-is', without any express or implied
          warranty.  In no event will the authors be held liable for any damages
          arising from the use of this software.

          Permission is granted to anyone to use this software for any purpose,
          including commercial applications, and to alter it and redistribute it
          freely, subject to the following restrictions:

          1. The origin of this software must not be misrepresented; you must not
             claim that you wrote the original software. If you use this software
             in a product, an acknowledgment in the product documentation would be
             appreciated but is not required.
          2. Altered source versions must be plainly marked as such, and must not be
             misrepresented as being the original software.
          3. This notice may not be removed or altered from any source distribution.
        */

        /**
         * \file SDL_gpu.h
         *
         * Include file for SDL GPU API functions
         */

        /* Type Declarations */

#if false
        // I think these are meant to be opaque types
        typedef struct SDL_GpuDevice SDL_GpuDevice;
        typedef struct SDL_GpuBuffer SDL_GpuBuffer;
        typedef struct SDL_GpuTransferBuffer SDL_GpuTransferBuffer;
        typedef struct SDL_GpuTexture SDL_GpuTexture;
        typedef struct SDL_GpuSampler SDL_GpuSampler;
        typedef struct SDL_GpuShader SDL_GpuShader;
        typedef struct SDL_GpuComputePipeline SDL_GpuComputePipeline;
        typedef struct SDL_GpuGraphicsPipeline SDL_GpuGraphicsPipeline;
        typedef struct SDL_GpuCommandBuffer SDL_GpuCommandBuffer;
        typedef struct SDL_GpuRenderPass SDL_GpuRenderPass;
        typedef struct SDL_GpuComputePass SDL_GpuComputePass;
        typedef struct SDL_GpuCopyPass SDL_GpuCopyPass;
        typedef struct SDL_GpuFence SDL_GpuFence;
#endif

        public enum SDL_GpuPrimitiveType
        {
            SDL_GPU_PRIMITIVETYPE_POINTLIST,
            SDL_GPU_PRIMITIVETYPE_LINELIST,
            SDL_GPU_PRIMITIVETYPE_LINESTRIP,
            SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
            SDL_GPU_PRIMITIVETYPE_TRIANGLESTRIP
        }

        public enum SDL_GpuLoadOp
        {
            SDL_GPU_LOADOP_LOAD,
            SDL_GPU_LOADOP_CLEAR,
            SDL_GPU_LOADOP_DONT_CARE
        }

        public enum SDL_GpuStoreOp
        {
            SDL_GPU_STOREOP_STORE,
            SDL_GPU_STOREOP_DONT_CARE
        }

        public enum SDL_GpuIndexElementSize
        {
            SDL_GPU_INDEXELEMENTSIZE_16BIT,
            SDL_GPU_INDEXELEMENTSIZE_32BIT
        }

        public enum SDL_GpuTextureFormat
        {
            SDL_GPU_TEXTUREFORMAT_INVALID = -1,

            /* Unsigned Normalized Float Color Formats */
            SDL_GPU_TEXTUREFORMAT_R8G8B8A8,
            SDL_GPU_TEXTUREFORMAT_B8G8R8A8,
            SDL_GPU_TEXTUREFORMAT_B5G6R5,
            SDL_GPU_TEXTUREFORMAT_B5G5R5A1,
            SDL_GPU_TEXTUREFORMAT_B4G4R4A4,
            SDL_GPU_TEXTUREFORMAT_R10G10B10A2,
            SDL_GPU_TEXTUREFORMAT_R16G16,
            SDL_GPU_TEXTUREFORMAT_R16G16B16A16,
            SDL_GPU_TEXTUREFORMAT_R8,
            SDL_GPU_TEXTUREFORMAT_A8,
            /* Compressed Unsigned Normalized Float Color Formats */
            SDL_GPU_TEXTUREFORMAT_BC1,
            SDL_GPU_TEXTUREFORMAT_BC2,
            SDL_GPU_TEXTUREFORMAT_BC3,
            SDL_GPU_TEXTUREFORMAT_BC7,
            /* Signed Normalized Float Color Formats  */
            SDL_GPU_TEXTUREFORMAT_R8G8_SNORM,
            SDL_GPU_TEXTUREFORMAT_R8G8B8A8_SNORM,
            /* Signed Float Color Formats */
            SDL_GPU_TEXTUREFORMAT_R16_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_R16G16_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_R16G16B16A16_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_R32_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_R32G32_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_R32G32B32A32_SFLOAT,
            /* Unsigned Integer Color Formats */
            SDL_GPU_TEXTUREFORMAT_R8_UINT,
            SDL_GPU_TEXTUREFORMAT_R8G8_UINT,
            SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UINT,
            SDL_GPU_TEXTUREFORMAT_R16_UINT,
            SDL_GPU_TEXTUREFORMAT_R16G16_UINT,
            SDL_GPU_TEXTUREFORMAT_R16G16B16A16_UINT,
            /* SRGB Color Formats */
            SDL_GPU_TEXTUREFORMAT_R8G8B8A8_SRGB,
            SDL_GPU_TEXTUREFORMAT_B8G8R8A8_SRGB,
            /* Compressed SRGB Color Formats */
            SDL_GPU_TEXTUREFORMAT_BC3_SRGB,
            SDL_GPU_TEXTUREFORMAT_BC7_SRGB,
            /* Depth Formats */
            SDL_GPU_TEXTUREFORMAT_D16_UNORM,
            SDL_GPU_TEXTUREFORMAT_D24_UNORM,
            SDL_GPU_TEXTUREFORMAT_D32_SFLOAT,
            SDL_GPU_TEXTUREFORMAT_D24_UNORM_S8_UINT,
            SDL_GPU_TEXTUREFORMAT_D32_SFLOAT_S8_UINT
        }

        public enum SDL_GpuTextureUsageFlagBits
        {
            SDL_GPU_TEXTUREUSAGE_SAMPLER_BIT = 0x00000001,
            SDL_GPU_TEXTUREUSAGE_COLOR_TARGET_BIT = 0x00000002,
            SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET_BIT = 0x00000004,
            SDL_GPU_TEXTUREUSAGE_GRAPHICS_STORAGE_READ_BIT = 0x00000008,
            SDL_GPU_TEXTUREUSAGE_COMPUTE_STORAGE_READ_BIT = 0x00000020,
            SDL_GPU_TEXTUREUSAGE_COMPUTE_STORAGE_WRITE_BIT = 0x00000040
        }

        public enum SDL_GpuTextureType
        {
            SDL_GPU_TEXTURETYPE_2D,
            SDL_GPU_TEXTURETYPE_3D,
            SDL_GPU_TEXTURETYPE_CUBE,
        }

        public enum SDL_GpuSampleCount
        {
            SDL_GPU_SAMPLECOUNT_1,
            SDL_GPU_SAMPLECOUNT_2,
            SDL_GPU_SAMPLECOUNT_4,
            SDL_GPU_SAMPLECOUNT_8
        }

        public enum SDL_GpuCubeMapFace
        {
            SDL_GPU_CUBEMAPFACE_POSITIVEX,
            SDL_GPU_CUBEMAPFACE_NEGATIVEX,
            SDL_GPU_CUBEMAPFACE_POSITIVEY,
            SDL_GPU_CUBEMAPFACE_NEGATIVEY,
            SDL_GPU_CUBEMAPFACE_POSITIVEZ,
            SDL_GPU_CUBEMAPFACE_NEGATIVEZ
        }

        public enum SDL_GpuBufferUsageFlagBits
        {
            SDL_GPU_BUFFERUSAGE_VERTEX_BIT = 0x00000001,
            SDL_GPU_BUFFERUSAGE_INDEX_BIT = 0x00000002,
            SDL_GPU_BUFFERUSAGE_INDIRECT_BIT = 0x00000004,
            SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ_BIT = 0x00000008,
            SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_READ_BIT = 0x00000020,
            SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_WRITE_BIT = 0x00000040
        }

        public enum SDL_GpuTransferBufferUsage
        {
            SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
            SDL_GPU_TRANSFERBUFFERUSAGE_DOWNLOAD
        }

        public enum SDL_GpuShaderStage
        {
            SDL_GPU_SHADERSTAGE_VERTEX,
            SDL_GPU_SHADERSTAGE_FRAGMENT
        }

        public enum SDL_GpuShaderFormat
        {
            SDL_GPU_SHADERFORMAT_INVALID,
            SDL_GPU_SHADERFORMAT_SPIRV,    /* Vulkan */
            SDL_GPU_SHADERFORMAT_HLSL,     /* D3D11, D3D12 */
            SDL_GPU_SHADERFORMAT_DXBC,     /* D3D11, D3D12 */
            SDL_GPU_SHADERFORMAT_DXIL,     /* D3D12 */
            SDL_GPU_SHADERFORMAT_MSL,      /* Metal */
            SDL_GPU_SHADERFORMAT_METALLIB, /* Metal */
            SDL_GPU_SHADERFORMAT_SECRET    /* NDA'd platforms */
        }

        public enum SDL_GpuVertexElementFormat
        {
            SDL_GPU_VERTEXELEMENTFORMAT_UINT,
            SDL_GPU_VERTEXELEMENTFORMAT_FLOAT,
            SDL_GPU_VERTEXELEMENTFORMAT_VECTOR2,
            SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3,
            SDL_GPU_VERTEXELEMENTFORMAT_VECTOR4,
            SDL_GPU_VERTEXELEMENTFORMAT_COLOR,
            SDL_GPU_VERTEXELEMENTFORMAT_BYTE4,
            SDL_GPU_VERTEXELEMENTFORMAT_SHORT2,
            SDL_GPU_VERTEXELEMENTFORMAT_SHORT4,
            SDL_GPU_VERTEXELEMENTFORMAT_NORMALIZEDSHORT2,
            SDL_GPU_VERTEXELEMENTFORMAT_NORMALIZEDSHORT4,
            SDL_GPU_VERTEXELEMENTFORMAT_HALFVECTOR2,
            SDL_GPU_VERTEXELEMENTFORMAT_HALFVECTOR4
        }

        public enum SDL_GpuVertexInputRate
        {
            SDL_GPU_VERTEXINPUTRATE_VERTEX = 0,
            SDL_GPU_VERTEXINPUTRATE_INSTANCE = 1
        }

        public enum SDL_GpuFillMode
        {
            SDL_GPU_FILLMODE_FILL,
            SDL_GPU_FILLMODE_LINE
        }

        public enum SDL_GpuCullMode
        {
            SDL_GPU_CULLMODE_NONE,
            SDL_GPU_CULLMODE_FRONT,
            SDL_GPU_CULLMODE_BACK
        }

        public enum SDL_GpuFrontFace
        {
            SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE,
            SDL_GPU_FRONTFACE_CLOCKWISE
        }

        public enum SDL_GpuCompareOp
        {
            SDL_GPU_COMPAREOP_NEVER,
            SDL_GPU_COMPAREOP_LESS,
            SDL_GPU_COMPAREOP_EQUAL,
            SDL_GPU_COMPAREOP_LESS_OR_EQUAL,
            SDL_GPU_COMPAREOP_GREATER,
            SDL_GPU_COMPAREOP_NOT_EQUAL,
            SDL_GPU_COMPAREOP_GREATER_OR_EQUAL,
            SDL_GPU_COMPAREOP_ALWAYS
        }

        public enum SDL_GpuStencilOp
        {
            SDL_GPU_STENCILOP_KEEP,
            SDL_GPU_STENCILOP_ZERO,
            SDL_GPU_STENCILOP_REPLACE,
            SDL_GPU_STENCILOP_INCREMENT_AND_CLAMP,
            SDL_GPU_STENCILOP_DECREMENT_AND_CLAMP,
            SDL_GPU_STENCILOP_INVERT,
            SDL_GPU_STENCILOP_INCREMENT_AND_WRAP,
            SDL_GPU_STENCILOP_DECREMENT_AND_WRAP
        }

        public enum SDL_GpuBlendOp
        {
            SDL_GPU_BLENDOP_ADD,
            SDL_GPU_BLENDOP_SUBTRACT,
            SDL_GPU_BLENDOP_REVERSE_SUBTRACT,
            SDL_GPU_BLENDOP_MIN,
            SDL_GPU_BLENDOP_MAX
        }

        public enum SDL_GpuBlendFactor
        {
            SDL_GPU_BLENDFACTOR_ZERO,
            SDL_GPU_BLENDFACTOR_ONE,
            SDL_GPU_BLENDFACTOR_SRC_COLOR,
            SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_COLOR,
            SDL_GPU_BLENDFACTOR_DST_COLOR,
            SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_COLOR,
            SDL_GPU_BLENDFACTOR_SRC_ALPHA,
            SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
            SDL_GPU_BLENDFACTOR_DST_ALPHA,
            SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_ALPHA,
            SDL_GPU_BLENDFACTOR_CONSTANT_COLOR,
            SDL_GPU_BLENDFACTOR_ONE_MINUS_CONSTANT_COLOR,
            SDL_GPU_BLENDFACTOR_SRC_ALPHA_SATURATE
        }

        public enum SDL_GpuColorComponentFlagBits
        {
            SDL_GPU_COLORCOMPONENT_R_BIT = 0x00000001,
            SDL_GPU_COLORCOMPONENT_G_BIT = 0x00000002,
            SDL_GPU_COLORCOMPONENT_B_BIT = 0x00000004,
            SDL_GPU_COLORCOMPONENT_A_BIT = 0x00000008
        }

        public enum SDL_GpuFilter
        {
            SDL_GPU_FILTER_NEAREST,
            SDL_GPU_FILTER_LINEAR
        }

        public enum SDL_GpuSamplerMipmapMode
        {
            SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
            SDL_GPU_SAMPLERMIPMAPMODE_LINEAR
        }

        public enum SDL_GpuSamplerAddressMode
        {
            SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
            SDL_GPU_SAMPLERADDRESSMODE_MIRRORED_REPEAT,
            SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE
        }

/*
 * VSYNC:
 *   Waits for vblank before presenting.
 *   If there is a pending image to present, the new image is enqueued for presentation.
 *   Disallows tearing at the cost of visual latency.
 *   When using this present mode, AcquireSwapchainTexture will block if too many frames are in flight.
 * IMMEDIATE:
 *   Immediately presents.
 *   Lowest latency option, but tearing may occur.
 *   When using this mode, AcquireSwapchainTexture will return NULL if too many frames are in flight.
 * MAILBOX:
 *   Waits for vblank before presenting. No tearing is possible.
 *   If there is a pending image to present, the pending image is replaced by the new image.
 *   Similar to VSYNC, but with reduced visual latency.
 *   When using this mode, AcquireSwapchainTexture will return NULL if too many frames are in flight.
 */
        public enum SDL_GpuPresentMode
        {
            SDL_GPU_PRESENTMODE_VSYNC,
            SDL_GPU_PRESENTMODE_IMMEDIATE,
            SDL_GPU_PRESENTMODE_MAILBOX
        }

/*
 * SDR:
 *   B8G8R8A8 or R8G8B8A8 swapchain. Pixel values are in nonlinear sRGB encoding. Blends raw pixel values.
 * SDR_LINEAR:
 *   B8G8R8A8_SRGB or R8G8B8A8_SRGB swapchain. Pixel values are in nonlinear sRGB encoding. Blends in linear space.
 * HDR_EXTENDED_LINEAR:
 *   R16G16B16A16_SFLOAT swapchain. Pixel values are in extended linear encoding. Blends in linear space.
 * HDR10_ST2048:
 *   A2R10G10B10 or A2B10G10R10 swapchain. Pixel values are in PQ ST2048 encoding. Blends raw pixel values. (TODO: verify this)
 */
        public enum SDL_GpuSwapchainComposition
        {
            SDL_GPU_SWAPCHAINCOMPOSITION_SDR,
            SDL_GPU_SWAPCHAINCOMPOSITION_SDR_LINEAR,
            SDL_GPU_SWAPCHAINCOMPOSITION_HDR_EXTENDED_LINEAR,
            SDL_GPU_SWAPCHAINCOMPOSITION_HDR10_ST2048
        }

        public enum SDL_GpuBackendBits : UInt64
        {
            SDL_GPU_BACKEND_INVALID = 0,
            SDL_GPU_BACKEND_VULKAN = 0x0000000000000001,
            SDL_GPU_BACKEND_D3D11 = 0x0000000000000002,
            SDL_GPU_BACKEND_METAL = 0x0000000000000004,
            SDL_GPU_BACKEND_ALL = (SDL_GPU_BACKEND_VULKAN | SDL_GPU_BACKEND_D3D11 | SDL_GPU_BACKEND_METAL)
        }


/* Structures */

        public struct SDL_GpuDepthStencilValue
        {
            public float depth;
            public UInt32 stencil;
        }

        public struct SDL_GpuRect
        {
            public Int32 x;
            public Int32 y;
            public Int32 w;
            public Int32 h;
        }

        public struct SDL_GpuColor
        {
            public float r;
            public float g;
            public float b;
            public float a;
        }

        public struct SDL_GpuViewport
        {
            public float x;
            public float y;
            public float w;
            public float h;
            public float minDepth;
            public float maxDepth;
        }

        public struct SDL_GpuTextureTransferInfo
        {
            public SDL_GpuTransferBuffer_Ptr transferBuffer;
            public UInt32 offset;      /* starting location of the image data */
            public UInt32 imagePitch;  /* number of pixels from one row to the next */
            public UInt32 imageHeight; /* number of rows from one layer/depth-slice to the next */
        }

        public struct SDL_GpuTransferBufferLocation
        {
            public SDL_GpuTransferBuffer_Ptr transferBuffer;
            public UInt32 offset;
        }

        public struct SDL_GpuTransferBufferRegion
        {
            public SDL_GpuTransferBuffer_Ptr transferBuffer;
            public UInt32 offset;
            public UInt32 size;
        }

        public struct SDL_GpuTextureSlice
        {
            public SDL_GpuTexture_Ptr texture;
            public UInt32 mipLevel;
            public UInt32 layer;
        }

        public struct SDL_GpuTextureLocation
        {
            public SDL_GpuTextureSlice textureSlice;
            public UInt32 x;
            public UInt32 y;
            public UInt32 z;
        }

        public struct SDL_GpuTextureRegion
        {
            public SDL_GpuTextureSlice textureSlice;
            public UInt32 x;
            public UInt32 y;
            public UInt32 z;
            public UInt32 w;
            public UInt32 h;
            public UInt32 d;
        }

        public struct SDL_GpuBufferLocation
        {
            public SDL_GpuBuffer_Ptr buffer;
            public UInt32 offset;
        }

        public struct SDL_GpuBufferRegion
        {
            public SDL_GpuBuffer_Ptr buffer;
            public UInt32 offset;
            public UInt32 size;
        }

        public struct SDL_GpuIndirectDrawCommand
        {
            public UInt32 vertexCount;   /* number of vertices to draw */
            public UInt32 instanceCount; /* number of instances to draw */
            public UInt32 firstVertex;   /* index of the first vertex to draw */
            public UInt32 firstInstance; /* ID of the first instance to draw */
        }

        public struct SDL_GpuIndexedIndirectDrawCommand
        {
            public UInt32 indexCount;    /* number of vertices to draw */
            public UInt32 instanceCount; /* number of instances to draw */
            public UInt32 firstIndex;    /* base index within the index buffer */
            public UInt32 vertexOffset;  /* value added to vertex index before indexing into the vertex buffer */
            public UInt32 firstInstance; /* ID of the first instance to draw */
        }

/* State structures */

        public struct SDL_GpuSamplerCreateInfo
        {
            public SDL_GpuFilter minFilter;
            public SDL_GpuFilter magFilter;
            public SDL_GpuSamplerMipmapMode mipmapMode;
            public SDL_GpuSamplerAddressMode addressModeU;
            public SDL_GpuSamplerAddressMode addressModeV;
            public SDL_GpuSamplerAddressMode addressModeW;
            public float mipLodBias;
            public SDL_bool anisotropyEnable;
            public float maxAnisotropy;
            public SDL_bool compareEnable;
            public SDL_GpuCompareOp compareOp;
            public float minLod;
            public float maxLod;
        }

        public struct SDL_GpuVertexBinding
        {
            public UInt32 binding;
            public UInt32 stride;
            public SDL_GpuVertexInputRate inputRate;
            public UInt32 stepRate;
        }

        public struct SDL_GpuVertexAttribute
        {
            public UInt32 location;
            public UInt32 binding;
            public SDL_GpuVertexElementFormat format;
            public UInt32 offset;
        }

        public unsafe struct SDL_GpuVertexInputState
        {
            public /*const*/ SDL_GpuVertexBinding* vertexBindings;
            public UInt32 vertexBindingCount;
            public /*const*/ SDL_GpuVertexAttribute* vertexAttributes;
            public UInt32 vertexAttributeCount;
        }

        public struct SDL_GpuStencilOpState
        {
            public SDL_GpuStencilOp failOp;
            public SDL_GpuStencilOp passOp;
            public SDL_GpuStencilOp depthFailOp;
            public SDL_GpuCompareOp compareOp;
        }

        public struct SDL_GpuColorAttachmentBlendState
        {
            public SDL_bool blendEnable;
            public SDL_GpuBlendFactor srcColorBlendFactor;
            public SDL_GpuBlendFactor dstColorBlendFactor;
            public SDL_GpuBlendOp colorBlendOp;
            public SDL_GpuBlendFactor srcAlphaBlendFactor;
            public SDL_GpuBlendFactor dstAlphaBlendFactor;
            public SDL_GpuBlendOp alphaBlendOp;
            public SDL_GpuColorComponentFlags colorWriteMask;
        }

        public unsafe struct SDL_GpuShaderCreateInfo
        {
            public size_t codeSize;
            public /*const*/ byte* code;
            public /*const*/ char* entryPointName;
            public SDL_GpuShaderFormat format;
            public SDL_GpuShaderStage stage;
            public UInt32 samplerCount;
            public UInt32 storageTextureCount;
            public UInt32 storageBufferCount;
            public UInt32 uniformBufferCount;
        }

        public struct SDL_GpuTextureCreateInfo
        {
            public UInt32 width;
            public UInt32 height;
            public UInt32 depth;
            public SDL_bool isCube;
            public UInt32 layerCount;
            public UInt32 levelCount;
            public SDL_GpuSampleCount sampleCount;
            public SDL_GpuTextureFormat format;
            public SDL_GpuTextureUsageFlags usageFlags;
        }

/* Pipeline state structures */

        public struct SDL_GpuRasterizerState
        {
            public SDL_GpuFillMode fillMode;
            public SDL_GpuCullMode cullMode;
            public SDL_GpuFrontFace frontFace;
            public SDL_bool depthBiasEnable;
            public float depthBiasConstantFactor;
            public float depthBiasClamp;
            public float depthBiasSlopeFactor;
        }

        public struct SDL_GpuMultisampleState
        {
            public SDL_GpuSampleCount multisampleCount;
            public UInt32 sampleMask;
        }

        public struct SDL_GpuDepthStencilState
        {
            public SDL_bool depthTestEnable;
            public SDL_bool depthWriteEnable;
            public SDL_GpuCompareOp compareOp;
            public SDL_bool stencilTestEnable;
            public SDL_GpuStencilOpState backStencilState;
            public SDL_GpuStencilOpState frontStencilState;
            public UInt32 compareMask;
            public UInt32 writeMask;
            public UInt32 reference;
        }

        public struct SDL_GpuColorAttachmentDescription
        {
            public SDL_GpuTextureFormat format;
            public SDL_GpuColorAttachmentBlendState blendState;
        }

        public unsafe struct SDL_GpuGraphicsPipelineAttachmentInfo
        {
            public SDL_GpuColorAttachmentDescription* colorAttachmentDescriptions;
            public UInt32 colorAttachmentCount;
            public SDL_bool hasDepthStencilAttachment;
            public SDL_GpuTextureFormat depthStencilFormat;
        }

        public unsafe struct SDL_GpuGraphicsPipelineCreateInfo
        {
            public SDL_GpuShader_Ptr vertexShader;
            public SDL_GpuShader_Ptr fragmentShader;
            public SDL_GpuVertexInputState vertexInputState;
            public SDL_GpuPrimitiveType primitiveType;
            public SDL_GpuRasterizerState rasterizerState;
            public SDL_GpuMultisampleState multisampleState;
            public SDL_GpuDepthStencilState depthStencilState;
            public SDL_GpuGraphicsPipelineAttachmentInfo attachmentInfo;
            public fixed float blendConstants[4];
        }

        public unsafe struct SDL_GpuComputePipelineCreateInfo
        {
            public size_t codeSize;
            public readonly byte* code; // was `const Uint8*`
            public readonly char* entryPointName; // was `const char*`
            public SDL_GpuShaderFormat format;
            public UInt32 readOnlyStorageTextureCount;
            public UInt32 readOnlyStorageBufferCount;
            public UInt32 readWriteStorageTextureCount;
            public UInt32 readWriteStorageBufferCount;
            public UInt32 uniformBufferCount;
            public UInt32 threadCountX;
            public UInt32 threadCountY;
            public UInt32 threadCountZ;
        }

        public struct SDL_GpuColorAttachmentInfo
        {
            /* The texture slice that will be used as a color attachment by a render pass. */
            public SDL_GpuTextureSlice textureSlice;

            /* Can be ignored by RenderPass if CLEAR is not used */
            public SDL_GpuColor clearColor;

            /* Determines what is done with the texture slice at the beginning of the render pass.
             *
             *   LOAD:
             *     Loads the data currently in the texture slice.
             *
             *   CLEAR:
             *     Clears the texture slice to a single color.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the texture slice memory.
             *     This is a good option if you know that every single pixel will be touched in the render pass.
             */
            public SDL_GpuLoadOp loadOp;

            /* Determines what is done with the texture slice at the end of the render pass.
             *
             *   STORE:
             *     Stores the results of the render pass in the texture slice.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the texture slice memory.
             *     This is often a good option for depth/stencil textures.
             */
            public SDL_GpuStoreOp storeOp;

            /* if SDL_TRUE, cycles the texture if the texture slice is bound and loadOp is not LOAD */
            public SDL_bool cycle;
        }

        public struct SDL_GpuDepthStencilAttachmentInfo
        {
            /* The texture slice that will be used as the depth stencil attachment by a render pass. */
            public SDL_GpuTextureSlice textureSlice;

            /* Can be ignored by the render pass if CLEAR is not used */
            public SDL_GpuDepthStencilValue depthStencilClearValue;

            /* Determines what is done with the depth values at the beginning of the render pass.
             *
             *   LOAD:
             *     Loads the depth values currently in the texture slice.
             *
             *   CLEAR:
             *     Clears the texture slice to a single depth.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the memory.
             *     This is a good option if you know that every single pixel will be touched in the render pass.
             */
            public SDL_GpuLoadOp loadOp;

            /* Determines what is done with the depth values at the end of the render pass.
             *
             *   STORE:
             *     Stores the depth results in the texture slice.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the texture slice memory.
             *     This is often a good option for depth/stencil textures.
             */
            public SDL_GpuStoreOp storeOp;

            /* Determines what is done with the stencil values at the beginning of the render pass.
             *
             *   LOAD:
             *     Loads the stencil values currently in the texture slice.
             *
             *   CLEAR:
             *     Clears the texture slice to a single stencil value.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the memory.
             *     This is a good option if you know that every single pixel will be touched in the render pass.
             */
            public SDL_GpuLoadOp stencilLoadOp;

            /* Determines what is done with the stencil values at the end of the render pass.
             *
             *   STORE:
             *     Stores the stencil results in the texture slice.
             *
             *   DONT_CARE:
             *     The driver will do whatever it wants with the texture slice memory.
             *     This is often a good option for depth/stencil textures.
             */
            public SDL_GpuStoreOp stencilStoreOp;

            /* if SDL_TRUE, cycles the texture if the texture slice is bound and any load ops are not LOAD */
            public SDL_bool cycle;
        }

/* Binding structs */

        public struct SDL_GpuBufferBinding
        {
            public SDL_GpuBuffer_Ptr buffer;
            public UInt32 offset;
        }

        public struct SDL_GpuTextureSamplerBinding
        {
            public SDL_GpuTexture_Ptr texture;
            public SDL_GpuSampler_Ptr sampler;
        }

        public struct SDL_GpuStorageBufferReadWriteBinding
        {
            public SDL_GpuBuffer_Ptr buffer;

            /* if SDL_TRUE, cycles the buffer if it is bound. */
            public SDL_bool cycle;
        }

        public struct SDL_GpuStorageTextureReadWriteBinding
        {
            public SDL_GpuTextureSlice textureSlice;

            /* if SDL_TRUE, cycles the texture if the texture slice is bound. */
            public SDL_bool cycle;
        }

        /* Functions */

        /* Device */

        /**
         * Creates a GPU context.
         *
         * Backends will first be checked for availability in order of bitflags passed using preferredBackends. If none of the backends are available, the remaining backends are checked as fallback renderers.
         *
         * Think of "preferred" backends as those that have pre-built shaders readily available - for example, you would set the SDL_GPU_BACKEND_VULKAN bit if your game includes SPIR-V shaders. If you generate shaders at runtime (i.e. via SDL_shader) and the library does _not_ provide you with a preferredBackends value, you should pass SDL_GPU_BACKEND_ALL so that updated versions of SDL can be aware of which backends the application was aware of at compile time. SDL_GPU_BACKEND_INVALID is an accepted value but is not recommended.
         *
         * \param preferredBackends a bitflag containing the renderers most recognized by the application
         * \param debugMode enable debug mode properties and validations
         * \param preferLowPower set this to SDL_TRUE if your app prefers energy efficiency over maximum GPU performance
         * \returns a GPU context on success or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSelectBackend
         * \sa SDL_GpuDestroyDevice
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuDevice_Ptr SDL_GpuCreateDevice(
            SDL_GpuBackend preferredBackends,
            SDL_bool debugMode,
            SDL_bool preferLowPower);

        /**
         * Destroys a GPU context previously returned by SDL_GpuCreateDevice.
         *
         * \param device a GPU Context to destroy
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuCreateDevice
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDestroyDevice(SDL_GpuDevice_Ptr device);

        /**
         * Returns the backend used to create this GPU context.
         *
         * \param device a GPU context to query
         * \returns an SDL_GpuBackend value, or SDL_GPU_BACKEND_INVALID on error
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSelectBackend
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuBackend SDL_GpuGetBackend(SDL_GpuDevice_Ptr device);

        /* State Creation */

        /**
         * Creates a pipeline object to be used in a compute workflow.
         *
         * Shader resource bindings must be authored to follow a particular order.
         * For SPIR-V shaders, use the following resource sets:
         *  0: Read-only storage textures, followed by read-only storage buffers
         *  1: Read-write storage textures, followed by read-write storage buffers
         *  2: Uniform buffers
         *
         * For HLSL/DXBC/DXIL, use the following order:
         *  For t registers:
         *   Read-only storage textures, followed by read-only storage buffers
         *  For b registers:
         *   Uniform buffers
         *  For u registers:
         *   Read-write storage textures, followed by read-write storage buffers
         *
         * For MSL/metallib, use the following order:
         *  For [[buffer]]:
         *   Uniform buffers, followed by read-only storage buffers, followed by read-write storage buffers
         *  For [[texture]]:
         *   Read-only storage textures, followed by read-write storage textures
         *
         * \param device a GPU Context
         * \param computePipelineCreateInfo a struct describing the state of the requested compute pipeline
         * \returns a compute pipeline object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuBindComputePipeline
         * \sa SDL_GpuReleaseComputePipeline
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuComputePipeline_Ptr SDL_GpuCreateComputePipeline(
            SDL_GpuDevice_Ptr device,
            SDL_GpuComputePipelineCreateInfo* computePipelineCreateInfo);

        /**
         * Creates a pipeline object to be used in a graphics workflow.
         *
         * \param device a GPU Context
         * \param pipelineCreateInfo a struct describing the state of the desired graphics pipeline
         * \returns a graphics pipeline object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuCreateShader
         * \sa SDL_GpuBindGraphicsPipeline
         * \sa SDL_GpuReleaseGraphicsPipeline
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuGraphicsPipeline_Ptr SDL_GpuCreateGraphicsPipeline(
            SDL_GpuDevice_Ptr device,
            SDL_GpuGraphicsPipelineCreateInfo* pipelineCreateInfo);

        /**
         * Creates a sampler object to be used when binding textures in a graphics workflow.
         *
         * \param device a GPU Context
         * \param samplerCreateInfo a struct describing the state of the desired sampler
         * \returns a sampler object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuBindVertexSamplers
         * \sa SDL_GpuBindFragmentSamplers
         * \sa SDL_ReleaseSampler
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuSampler_Ptr SDL_GpuCreateSampler(
            SDL_GpuDevice_Ptr device,
            SDL_GpuSamplerCreateInfo* samplerCreateInfo);

        /**
         * Creates a shader to be used when creating a graphics pipeline.
         *
         * Shader resource bindings must be authored to follow a particular order.
         * For SPIR-V shaders, use the following resource sets:
         *  For vertex shaders:
         *   0: Sampled textures, followed by storage textures, followed by storage buffers
         *   1: Uniform buffers
         *  For fragment shaders:
         *   2: Sampled textures, followed by storage textures, followed by storage buffers
         *   3: Uniform buffers
         *
         * For HLSL/DXBC/DXIL, use the following order:
         *  For t registers:
         *   Sampled textures, followed by storage textures, followed by storage buffers
         *  For s registers:
         *   Samplers with indices corresponding to the sampled textures
         *  For b registers:
         *   Uniform buffers
         *
         * For MSL/metallib, use the following order:
         *  For [[texture]]:
         *   Sampled textures, followed by storage textures
         *  For [[sampler]]:
         *   Samplers with indices corresponding to the sampled textures
         *  For [[buffer]]:
         *   Uniform buffers, followed by storage buffers.
         *   Vertex buffer 0 is bound at [[buffer(30)]], vertex buffer 1 at [[buffer(29)]], and so on.
         *    Rather than manually authoring vertex buffer indices, use the [[stage_in]] attribute
         *    which will automatically use the vertex input information from the SDL_GpuPipeline.
         *
         * \param device a GPU Context
         * \param shaderCreateInfo a struct describing the state of the desired shader
         * \returns a shader object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuCreateGraphicsPipeline
         * \sa SDL_GpuReleaseShader
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuShader_Ptr SDL_GpuCreateShader(
            SDL_GpuDevice_Ptr device,
            SDL_GpuShaderCreateInfo* shaderCreateInfo);

        /**
         * Creates a texture object to be used in graphics or compute workflows.
         * The contents of this texture are undefined until data is written to the texture.
         *
         * Note that certain combinations of usage flags are invalid.
         * For example, a texture cannot have both the SAMPLER and GRAPHICS_STORAGE_READ flags.
         *
         * If you request a sample count higher than the hardware supports,
         * the implementation will automatically fall back to the highest available sample count.
         *
         * \param device a GPU Context
         * \param textureCreateInfo a struct describing the state of the texture to create
         * \returns a texture object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuUploadToTexture
         * \sa SDL_GpuDownloadFromTexture
         * \sa SDL_GpuBindVertexSamplers
         * \sa SDL_GpuBindVertexStorageTextures
         * \sa SDL_GpuBindFragmentSamplers
         * \sa SDL_GpuBindFragmentStorageTextures
         * \sa SDL_GpuBindComputeStorageTextures
         * \sa SDL_GpuBlit
         * \sa SDL_GpuReleaseTexture
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuTexture_Ptr SDL_GpuCreateTexture(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTextureCreateInfo* textureCreateInfo);

        /**
         * Creates a buffer object to be used in graphics or compute workflows.
         * The contents of this buffer are undefined until data is written to the buffer.
         *
         * Note that certain combinations of usage flags are invalid.
         * For example, a buffer cannot have both the VERTEX and INDEX flags.
         *
         * \param device a GPU Context
         * \param usageFlags bitflag mask hinting at how the buffer will be used
         * \param sizeInBytes the size of the buffer
         * \returns a buffer object on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuUploadToBuffer
         * \sa SDL_GpuBindVertexBuffers
         * \sa SDL_GpuBindIndexBuffer
         * \sa SDL_GpuBindVertexStorageBuffers
         * \sa SDL_GpuBindFragmentStorageBuffers
         * \sa SDL_GpuBindComputeStorageBuffers
         * \sa SDL_GpuReleaseBuffer
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuBuffer_Ptr SDL_GpuCreateBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuBufferUsageFlags usageFlags,
            UInt32 sizeInBytes);

        /**
         * Creates a transfer buffer to be used when uploading to or downloading from graphics resources.
         *
         * \param device a GPU Context
         * \param usage whether the transfer buffer will be used for uploads or downloads
         * \param sizeInBytes the size of the transfer buffer
         * \returns a transfer buffer on success, or NULL on failure
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuUploadToBuffer
         * \sa SDL_GpuDownloadFromBuffer
         * \sa SDL_GpuUploadToTexture
         * \sa SDL_GpuDownloadFromTexture
         * \sa SDL_GpuReleaseTransferBuffer
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuTransferBuffer_Ptr SDL_GpuCreateTransferBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTransferBufferUsage usage,
            UInt32 sizeInBytes);

        /* Debug Naming */

        /**
         * Sets an arbitrary string constant to label a buffer. Useful for debugging.
         *
         * \param device a GPU Context
         * \param buffer a buffer to attach the name to
         * \param text a UTF-8 string constant to mark as the name of the buffer
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuSetBufferName(
            SDL_GpuDevice_Ptr device,
            SDL_GpuBuffer_Ptr buffer,
            /*const*/ char* text);

        /**
         * Sets an arbitrary string constant to label a texture. Useful for debugging.
         *
         * \param device a GPU Context
         * \param texture a texture to attach the name to
         * \param text a UTF-8 string constant to mark as the name of the texture
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuSetTextureName(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTexture_Ptr texture,
            /*const*/ char* text);

        /**
         * Inserts an arbitrary string label into the command buffer callstream.
         * Useful for debugging.
         *
         * \param commandBuffer a command buffer
         * \param text a UTF-8 string constant to insert as the label
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuInsertDebugLabel(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            /*const*/ char* text);

        /**
         * Begins a debug group with an arbitary name.
         * Used for denoting groups of calls when viewing the command buffer callstream
         * in a graphics debugging tool.
         *
         * Each call to SDL_GpuPushDebugGroup must have a corresponding call to SDL_GpuPopDebugGroup.
         *
         * On some backends (e.g. Metal), pushing a debug group during a render/blit/compute pass
         * will create a group that is scoped to the native pass rather than the command buffer.
         * For best results, if you push a debug group during a pass, always pop it in the same pass.
         *
         * \param commandBuffer a command buffer
         * \param name a UTF-8 string constant that names the group
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuPopDebugGroup
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuPushDebugGroup(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            /*const*/ char* name);

        /**
         * Ends the most-recently pushed debug group.
         *
         * \param commandBuffer a command buffer
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuPushDebugGroup
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuPopDebugGroup(
            SDL_GpuCommandBuffer_Ptr commandBuffer);

        /* Disposal */

        /**
         * Frees the given texture as soon as it is safe to do so.
         * You must not reference the texture after calling this function.
         *
         * \param device a GPU context
         * \param texture a texture to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseTexture(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTexture_Ptr texture);

        /**
         * Frees the given sampler as soon as it is safe to do so.
         * You must not reference the texture after calling this function.
         *
         * \param device a GPU context
         * \param sampler a sampler to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseSampler(
            SDL_GpuDevice_Ptr device,
            SDL_GpuSampler_Ptr sampler);

        /**
         * Frees the given buffer as soon as it is safe to do so.
         * You must not reference the buffer after calling this function.
         *
         * \param device a GPU context
         * \param buffer a buffer to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuBuffer_Ptr buffer);

        /**
         * Frees the given transfer buffer as soon as it is safe to do so.
         * You must not reference the transfer buffer after calling this function.
         *
         * \param device a GPU context
         * \param transferBuffer a transfer buffer to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseTransferBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTransferBuffer_Ptr transferBuffer);

        /**
         * Frees the given compute pipeline as soon as it is safe to do so.
         * You must not reference the compute pipeline after calling this function.
         *
         * \param device a GPU context
         * \param computePipeline a compute pipeline to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseComputePipeline(
            SDL_GpuDevice_Ptr device,
            SDL_GpuComputePipeline_Ptr computePipeline);

        /**
         * Frees the given shader as soon as it is safe to do so.
         * You must not reference the shader after calling this function.
         *
         * \param device a GPU context
         * \param shader a shader to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseShader(
            SDL_GpuDevice_Ptr device,
            SDL_GpuShader_Ptr shader);

        /**
         * Frees the given graphics pipeline as soon as it is safe to do so.
         * You must not reference the graphics pipeline after calling this function.
         *
         * \param device a GPU context
         * \param graphicsPipeline a graphics pipeline to be destroyed
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseGraphicsPipeline(
            SDL_GpuDevice_Ptr device,
            SDL_GpuGraphicsPipeline_Ptr graphicsPipeline);

        /*
         * COMMAND BUFFERS
         *
         * Render state is managed via command buffers.
         * When setting render state, that state is always local to the command buffer.
         *
         * Commands only begin execution on the GPU once Submit is called.
         * Once the command buffer is submitted, it is no longer valid to use it.
         *
         * In multi-threading scenarios, you should acquire and submit a command buffer on the same thread.
         * As long as you satisfy this requirement, all functionality related to command buffers is thread-safe.
         */

        /**
         * Acquire a command buffer.
         * This command buffer is managed by the implementation and should not be freed by the user.
         * The command buffer may only be used on the thread it was acquired on.
         * The command buffer should be submitted on the thread it was acquired on.
         *
         * \param device a GPU context
         * \returns a command buffer
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSubmit
         * \sa SDL_GpuSubmitAndAcquireFence
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuCommandBuffer_Ptr SDL_GpuAcquireCommandBuffer(
            SDL_GpuDevice_Ptr device);

        /*
         * UNIFORM DATA
         *
         * Uniforms are for passing data to shaders.
         * The uniform data will be constant across all executions of the shader.
         *
         * There are 4 available uniform slots per shader stage (vertex, fragment, compute).
         * Uniform data pushed to a slot on a stage keeps its value throughout the command buffer
         * until you call the relevant Push function on that slot again.
         *
         * For example, you could write your vertex shaders to read a camera matrix from uniform binding slot 0,
         * push the camera matrix at the start of the command buffer, and that data will be used for every
         * subsequent draw call.
         *
         * It is valid to push uniform data during a render or compute pass.
         *
         * Uniforms are best for pushing small amounts of data.
         * If you are pushing more than a matrix or two per call you should consider using a storage buffer instead.
         */

        /**
         * Pushes data to a vertex uniform slot on the command buffer.
         * Subsequent draw calls will use this uniform data.
         *
         * \param commandBuffer a command buffer
         * \param slotIndex the vertex uniform slot to push data to
         * \param data client data to write
         * \param dataLengthInBytes the length of the data to write
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuPushVertexUniformData(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            UInt32 slotIndex,
            /*const*/ void* data,
            UInt32 dataLengthInBytes);

        /**
         * Pushes data to a fragment uniform slot on the command buffer.
         * Subsequent draw calls will use this uniform data.
         *
         * \param commandBuffer a command buffer
         * \param slotIndex the fragment uniform slot to push data to
         * \param data client data to write
         * \param dataLengthInBytes the length of the data to write
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuPushFragmentUniformData(
        SDL_GpuCommandBuffer_Ptr commandBuffer,
        UInt32 slotIndex,
        /*const*/ void* data,
        UInt32 dataLengthInBytes);

        /**
         * Pushes data to a uniform slot on the command buffer.
         * Subsequent draw calls will use this uniform data.
         *
         * \param commandBuffer a command buffer
         * \param slotIndex the uniform slot to push data to
         * \param data client data to write
         * \param dataLengthInBytes the length of the data to write
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuPushComputeUniformData(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            UInt32 slotIndex,
            /*const*/ void* data,
            UInt32 dataLengthInBytes);

        /*
         * A NOTE ON CYCLING
         *
         * When using a command buffer, operations do not occur immediately -
         * they occur some time after the command buffer is submitted.
         *
         * When a resource is used in a pending or active command buffer, it is considered to be "bound".
         * When a resource is no longer used in any pending or active command buffers, it is considered to be "unbound".
         *
         * If data resources are bound, it is unspecified when that data will be unbound
         * unless you acquire a fence when submitting the command buffer and wait on it.
         * However, this doesn't mean you need to track resource usage manually.
         *
         * All of the functions and structs that involve writing to a resource have a "cycle" bool.
         * GpuTransferBuffer, GpuBuffer, and GpuTexture all effectively function as ring buffers on internal resources.
         * When cycle is SDL_TRUE, if the resource is bound, the cycle rotates to the next unbound internal resource,
         * or if none are available, a new one is created.
         * This means you don't have to worry about complex state tracking and synchronization as long as cycling is correctly employed.
         *
         * For example: you can call SetTransferData and then UploadToTexture. The next time you call SetTransferData,
         * if you set the cycle param to SDL_TRUE, you don't have to worry about overwriting any data that is not yet uploaded.
         *
         * Another example: If you are using a texture in a render pass every frame, this can cause a data dependency between frames.
         * If you set cycle to SDL_TRUE in the ColorAttachmentInfo struct, you can prevent this data dependency.
         *
         * Note that all functions which write to a texture specifically write to a GpuTextureSlice,
         * and these slices themselves are tracked for binding.
         * The GpuTexture will only cycle if the specific GpuTextureSlice being written to is bound.
         *
         * Cycling will never undefine already bound data.
         * When cycling, all data in the resource is considered to be undefined for subsequent commands until that data is written again.
         * You must take care not to read undefined data.
         *
         * You must also take care not to overwrite a section of data that has been referenced in a command without cycling first.
         * It is OK to overwrite unreferenced data in a bound resource without cycling,
         * but overwriting a section of data that has already been referenced will produce unexpected results.
         */

        /* Graphics State */

        /**
         * Begins a render pass on a command buffer.
         * A render pass consists of a set of texture slices, clear values, and load/store operations
         * which will be rendered to during the render pass.
         * All operations related to graphics pipelines must take place inside of a render pass.
         * A default viewport and scissor state are automatically set when this is called.
         * You cannot begin another render pass, or begin a compute pass or copy pass
         * until you have ended the render pass.
         *
         * \param commandBuffer a command buffer
         * \param colorAttachmentInfos an array of SDL_GpuColorAttachmentInfo structs
         * \param colorAttachmentCount the number of color attachments in the colorAttachmentInfos array
         * \param depthStencilAttachmentInfo the depth-stencil target and clear value, may be NULL
         * \returns a render pass handle
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuEndRenderPass
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuRenderPass_Ptr SDL_GpuBeginRenderPass(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            SDL_GpuColorAttachmentInfo* colorAttachmentInfos,
            UInt32 colorAttachmentCount,
            SDL_GpuDepthStencilAttachmentInfo* depthStencilAttachmentInfo);

        /**
         * Binds a graphics pipeline on a render pass to be used in rendering.
         * A graphics pipeline must be bound before making any draw calls.
         *
         * \param renderPass a render pass handle
         * \param graphicsPipeline the graphics pipeline to bind
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuBindGraphicsPipeline(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuGraphicsPipeline_Ptr graphicsPipeline);

        /**
         * Sets the current viewport state on a command buffer.
         *
         * \param renderPass a render pass handle
         * \param viewport the viewport to set
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuSetViewport(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuViewport* viewport);

        /**
         * Sets the current scissor state on a command buffer.
         *
         * \param renderPass a render pass handle
         * \param scissor the scissor area to set
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuSetScissor(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuRect* scissor);

        /**
         * Binds vertex buffers on a command buffer for use with subsequent draw calls.
         *
         * \param renderPass a render pass handle
         * \param firstBinding the starting bind point for the vertex buffers
         * \param pBindings an array of SDL_GpuBufferBinding structs containing vertex buffers and offset values
         * \param bindingCount the number of bindings in the pBindings array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindVertexBuffers(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstBinding,
            SDL_GpuBufferBinding* pBindings,
            UInt32 bindingCount);

        /**
         * Binds an index buffer on a command buffer for use with subsequent draw calls.
         *
         * \param renderPass a render pass handle
         * \param pBinding a pointer to a struct containing an index buffer and offset
         * \param indexElementSize whether the index values in the buffer are 16- or 32-bit
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindIndexBuffer(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuBufferBinding* pBinding,
            SDL_GpuIndexElementSize indexElementSize);

        /**
         * Binds texture-sampler pairs for use on the vertex shader.
         * The textures must have been created with SDL_GPU_TEXTUREUSAGE_SAMPLER_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the vertex sampler slot to begin binding from
         * \param textureSamplerBindings an array of texture-sampler binding structs
         * \param bindingCount the number of texture-sampler pairs to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindVertexSamplers(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuTextureSamplerBinding* textureSamplerBindings,
            UInt32 bindingCount);

        /**
         * Binds storage textures for use on the vertex shader.
         * These textures must have been created with SDL_GPU_TEXTUREUSAGE_GRAPHICS_STORAGE_READ_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the vertex storage texture slot to begin binding from
         * \param storageTextureSlices an array of storage texture slices
         * \param bindingCount the number of storage texture slices to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindVertexStorageTextures(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuTextureSlice* storageTextureSlices,
            UInt32 bindingCount);

        /**
         * Binds storage buffers for use on the vertex shader.
         * These buffers must have been created with SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the vertex storage buffer slot to begin binding from
         * \param storageBuffers an array of buffers
         * \param bindingCount the number of buffers to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindVertexStorageBuffers(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuBuffer_Ptr* storageBuffers,
            UInt32 bindingCount);

        /**
         * Binds texture-sampler pairs for use on the fragment shader.
         * The textures must have been created with SDL_GPU_TEXTUREUSAGE_SAMPLER_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the fragment sampler slot to begin binding from
         * \param textureSamplerBindings an array of texture-sampler binding structs
         * \param bindingCount the number of texture-sampler pairs to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindFragmentSamplers(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuTextureSamplerBinding* textureSamplerBindings,
            UInt32 bindingCount);

        /**
         * Binds storage textures for use on the fragment shader.
         * These textures must have been created with SDL_GPU_TEXTUREUSAGE_GRAPHICS_STORAGE_READ_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the fragment storage texture slot to begin binding from
         * \param storageTextureSlices an array of storage texture slices
         * \param bindingCount the number of storage texture slices to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindFragmentStorageTextures(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuTextureSlice* storageTextureSlices,
            UInt32 bindingCount);

        /**
         * Binds storage buffers for use on the fragment shader.
         * These buffers must have been created with SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ_BIT.
         *
         * \param renderPass a render pass handle
         * \param firstSlot the fragment storage buffer slot to begin binding from
         * \param storageBuffers an array of storage buffers
         * \param bindingCount the number of storage buffers to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindFragmentStorageBuffers(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 firstSlot,
            SDL_GpuBuffer_Ptr* storageBuffers,
            UInt32 bindingCount);

        /* Drawing */

        /**
         * Draws data using bound graphics state with an index buffer and instancing enabled.
         * You must not call this function before binding a graphics pipeline.
         *
         * \param renderPass a render pass handle
         * \param baseVertex the starting offset to read from the vertex buffer
         * \param startIndex the starting offset to read from the index buffer
         * \param primitiveCount the number of primitives to draw
         * \param instanceCount the number of instances that will be drawn
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDrawIndexedPrimitives(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 baseVertex,
            UInt32 startIndex,
            UInt32 primitiveCount,
            UInt32 instanceCount);

        /**
         * Draws data using bound graphics state.
         * You must not call this function before binding a graphics pipeline.
         *
         * \param renderPass a render pass handle
         * \param vertexStart The starting offset to read from the vertex buffer
         * \param primitiveCount The number of primitives to draw
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDrawPrimitives(
            SDL_GpuRenderPass_Ptr renderPass,
            UInt32 vertexStart,
            UInt32 primitiveCount);

        /**
         * Draws data using bound graphics state and with draw parameters set from a buffer.
         * The buffer layout should match the layout of SDL_GpuIndirectDrawCommand.
         * You must not call this function before binding a graphics pipeline.
         *
         * \param renderPass a render pass handle
         * \param buffer a buffer containing draw parameters
         * \param offsetInBytes the offset to start reading from the draw buffer
         * \param drawCount the number of draw parameter sets that should be read from the draw buffer
         * \param stride the byte stride between sets of draw parameters
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDrawPrimitivesIndirect(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuBuffer_Ptr buffer,
            UInt32 offsetInBytes,
            UInt32 drawCount,
            UInt32 stride);

        /**
         * Draws data using bound graphics state with an index buffer enabled
         * and with draw parameters set from a buffer.
         * The buffer layout should match the layout of SDL_GpuIndexedIndirectDrawCommand.
         * You must not call this function before binding a graphics pipeline.
         *
         * \param renderPass a render pass handle
         * \param buffer a buffer containing draw parameters
         * \param offsetInBytes the offset to start reading from the draw buffer
         * \param drawCount the number of draw parameter sets that should be read from the draw buffer
         * \param stride the byte stride between sets of draw parameters
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDrawIndexedPrimitivesIndirect(
            SDL_GpuRenderPass_Ptr renderPass,
            SDL_GpuBuffer_Ptr buffer,
            UInt32 offsetInBytes,
            UInt32 drawCount,
            UInt32 stride);

        /**
         * Ends the given render pass.
         * All bound graphics state on the render pass command buffer is unset.
         * The render pass handle is now invalid.
         *
         * \param renderPass a render pass handle
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuEndRenderPass(
            SDL_GpuRenderPass_Ptr renderPass);

        /* Compute Pass */

        /**
         * Begins a compute pass on a command buffer.
         * A compute pass is defined by a set of texture slices and buffers that
         * will be written to by compute pipelines.
         * These textures and buffers must have been created with the COMPUTE_STORAGE_WRITE bit.
         * If these resources will also be read during the pass, they must be created with the COMPUTE_STORAGE_READ bit.
         * All operations related to compute pipelines must take place inside of a compute pass.
         * You must not begin another compute pass, or a render pass or copy pass
         * before ending the compute pass.
         *
         * \param commandBuffer a command buffer
         * \param storageTextureBindings an array of writeable storage texture binding structs
         * \param storageTextureBindingCount the number of storage textures to bind from the array
         * \param storageBufferBindings an array of writeable storage buffer binding structs
         * \param storageBufferBindingCount an array of read-write storage buffer binding structs
         *
         * \returns a compute pass handle
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuEndComputePass
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe SDL_GpuComputePass_Ptr SDL_GpuBeginComputePass(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            SDL_GpuStorageTextureReadWriteBinding* storageTextureBindings,
            UInt32 storageTextureBindingCount,
            SDL_GpuStorageBufferReadWriteBinding* storageBufferBindings,
            UInt32 storageBufferBindingCount);

        /**
         * Binds a compute pipeline on a command buffer for use in compute dispatch.
         *
         * \param computePass a compute pass handle
         * \param computePipeline a compute pipeline to bind
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuBindComputePipeline(
            SDL_GpuComputePass_Ptr computePass,
            SDL_GpuComputePipeline_Ptr computePipeline);

        /**
         * Binds storage textures as readonly for use on the compute pipeline.
         * These textures must have been created with SDL_GPU_TEXTUREUSAGE_COMPUTE_STORAGE_READ_BIT.
         *
         * \param computePass a compute pass handle
         * \param firstSlot the compute storage texture slot to begin binding from
         * \param storageTextureSlices an array of storage texture binding structs
         * \param bindingCount the number of storage textures to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindComputeStorageTextures(
            SDL_GpuComputePass_Ptr computePass,
            UInt32 firstSlot,
            SDL_GpuTextureSlice* storageTextureSlices,
            UInt32 bindingCount);

        /**
         * Binds storage buffers as readonly for use on the compute pipeline.
         * These buffers must have been created with SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_READ_BIT.
         *
         * \param computePass a compute pass handle
         * \param firstSlot the compute storage buffer slot to begin binding from
         * \param storageBuffers an array of storage buffer binding structs
         * \param bindingCount the number of storage buffers to bind from the array
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBindComputeStorageBuffers(
            SDL_GpuComputePass_Ptr computePass,
            UInt32 firstSlot,
            SDL_GpuBuffer_Ptr* storageBuffers,
            UInt32 bindingCount);

        /**
         * Dispatches compute work.
         * You must not call this function before binding a compute pipeline.
         *
         * A VERY IMPORTANT NOTE
         * If you dispatch multiple times in a compute pass,
         * and the dispatches write to the same resource region as each other,
         * there is no guarantee of which order the writes will occur.
         * If the write order matters, you MUST end the compute pass and begin another one.
         *
         * \param computePass a compute pass handle
         * \param groupCountX number of local workgroups to dispatch in the X dimension
         * \param groupCountY number of local workgroups to dispatch in the Y dimension
         * \param groupCountZ number of local workgroups to dispatch in the Z dimension
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuDispatchCompute(
            SDL_GpuComputePass_Ptr computePass,
            UInt32 groupCountX,
            UInt32 groupCountY,
            UInt32 groupCountZ);

        /**
         * Ends the current compute pass.
         * All bound compute state on the command buffer is unset.
         * The compute pass handle is now invalid.
         *
         * \param computePass a compute pass handle
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuEndComputePass(
            SDL_GpuComputePass_Ptr computePass);

        /* TransferBuffer Data */

        /**
         * Maps a transfer buffer into application address space.
         * You must unmap the transfer buffer before encoding upload commands.
         *
         * \param device a GPU context
         * \param transferBuffer a transfer buffer
         * \param cycle if SDL_TRUE, cycles the transfer buffer if it is bound
         * \param ppData where to store the address of the mapped transfer buffer memory
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuMapTransferBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTransferBuffer_Ptr transferBuffer,
            SDL_bool cycle,
            void** ppData);

        /**
         * Unmaps a previously mapped transfer buffer.
         *
         * \param device a GPU context
         * \param transferBuffer a previously mapped transfer buffer
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuUnmapTransferBuffer(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTransferBuffer_Ptr transferBuffer);

        /**
         * Immediately copies data from a pointer to a transfer buffer.
         *
         * \param device a GPU context
         * \param source a pointer to data to copy into the transfer buffer
         * \param destination a transfer buffer with offset and size
         * \param cycle if SDL_TRUE, cycles the transfer buffer if it is bound, otherwise overwrites the data.
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuSetTransferData(
            SDL_GpuDevice_Ptr device,
            /*const*/ void* source,
            SDL_GpuTransferBufferRegion *destination,
            SDL_bool cycle);

        /**
         * Immediately copies data from a transfer buffer to a pointer.
         *
         * \param device a GPU context
         * \param source a transfer buffer with offset and size
         * \param destination a data pointer
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuGetTransferData(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTransferBufferRegion* source,
            void* destination);

        /* Copy Pass */

        /**
         * Begins a copy pass on a command buffer.
         * All operations related to copying to or from buffers or textures take place inside a copy pass.
         * You must not begin another copy pass, or a render pass or compute pass
         * before ending the copy pass.
         *
         * \param commandBuffer a command buffer
         * \returns a copy pass handle
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuCopyPass_Ptr SDL_GpuBeginCopyPass(
            SDL_GpuCommandBuffer_Ptr commandBuffer);

        /**
         * Uploads data from a transfer buffer to a texture.
         * The upload occurs on the GPU timeline.
         * You may assume that the upload has finished in subsequent commands.
         *
         * You must align the data in the transfer buffer to a multiple of
         * the texel size of the texture format.
         *
         * \param copyPass a copy pass handle
         * \param source the source transfer buffer with image layout information
         * \param destination the destination texture region
         * \param cycle if SDL_TRUE, cycles the texture if the texture slice is bound, otherwise overwrites the data.
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuUploadToTexture(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuTextureTransferInfo* source,
            SDL_GpuTextureRegion* destination,
            SDL_bool cycle);

        /* Uploads data from a TransferBuffer to a Buffer. */

        /**
         * Uploads data from a transfer buffer to a buffer.
         * The upload occurs on the GPU timeline.
         * You may assume that the upload has finished in subsequent commands.
         *
         * \param copyPass a copy pass handle
         * \param source the source transfer buffer with offset
         * \param destination the destination buffer with offset and size
         * \param cycle if SDL_TRUE, cycles the buffer if it is bound, otherwise overwrites the data.
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuUploadToBuffer(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuTransferBufferLocation* source,
            SDL_GpuBufferRegion* destination,
            SDL_bool cycle);

        /**
         * Performs a texture-to-texture copy.
         * This copy occurs on the GPU timeline.
         * You may assume the copy has finished in subsequent commands.
         *
         * \param copyPass a copy pass handle
         * \param source a source texture region
         * \param destination a destination texture region
         * \param w the width of the region to copy
         * \param h the height of the region to copy
         * \param d the depth of the region to copy
         * \param cycle if SDL_TRUE, cycles the destination texture if the destination texture slice is bound, otherwise overwrites the data.
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuCopyTextureToTexture(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuTextureLocation* source,
            SDL_GpuTextureLocation* destination,
            UInt32 w,
            UInt32 h,
            UInt32 d,
            SDL_bool cycle);

        /* Copies data from a buffer to a buffer. */

        /**
         * Performs a buffer-to-buffer copy.
         * This copy occurs on the GPU timeline.
         * You may assume the copy has finished in subsequent commands.
         *
         * \param copyPass a copy pass handle
         * \param source the buffer and offset to copy from
         * \param destination the buffer and offset to copy to
         * \param size the length of the buffer to copy
         * \param cycle if SDL_TRUE, cycles the destination buffer if it is bound, otherwise overwrites the data.
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuCopyBufferToBuffer(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuBufferLocation* source,
            SDL_GpuBufferLocation* destination,
            UInt32 size,
            SDL_bool cycle);

        /**
         * Generates mipmaps for the given texture.
         *
         * \param copyPass a copy pass handle
         * \param texture a texture with more than 1 mip level
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuGenerateMipmaps(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuTexture_Ptr texture);

        /**
         * Copies data from a texture to a transfer buffer on the GPU timeline.
         * This data is not guaranteed to be copied until the command buffer fence is signaled.
         *
         * \param copyPass a copy pass handle
         * \param source the source texture region
         * \param destination the destination transfer buffer with image layout information
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuDownloadFromTexture(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuTextureRegion* source,
            SDL_GpuTextureTransferInfo* destination);

        /**
         * Copies data from a buffer to a transfer buffer on the GPU timeline.
         * This data is not guaranteed to be copied until the command buffer fence is signaled.
         *
         * \param copyPass a copy pass handle
         * \param source the source buffer with offset and size
         * \param destination the destination transfer buffer with offset
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuDownloadFromBuffer(
            SDL_GpuCopyPass_Ptr copyPass,
            SDL_GpuBufferRegion* source,
            SDL_GpuTransferBufferLocation* destination);

        /**
         * Ends the current copy pass.
         *
         * \param copyPass a copy pass handle
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuEndCopyPass(
            SDL_GpuCopyPass_Ptr copyPass);

        /**
         * Blits from a source texture region to a destination texture region.
         * This function must not be called inside of any render, compute, or copy pass.
         *
         * \param commandBuffer a command buffer
         * \param source the texture region to copy from
         * \param destination the texture region to copy to
         * \param filterMode the filter mode that will be used when blitting
         * \param cycle if SDL_TRUE, cycles the destination texture if the destination texture slice is bound, otherwise overwrites the data.
         *
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuBlit(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            SDL_GpuTextureRegion* source,
            SDL_GpuTextureRegion* destination,
            SDL_GpuFilter filterMode,
            SDL_bool cycle);

        /* Submission/Presentation */

        /**
         * Obtains whether or not a swapchain composition is supported by the GPU backend.
         *
         * \param device a GPU context
         * \param window an SDL_Window
         * \param swapchainComposition the swapchain composition to check
         *
         * \returns SDL_TRUE if supported, SDL_FALSE if unsupported (or on error)
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuSupportsSwapchainComposition(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window,
            SDL_GpuSwapchainComposition swapchainComposition);

        /**
         * Obtains whether or not a presentation mode is supported by the GPU backend.
         *
         * \param device a GPU context
         * \param window an SDL_Window
         * \param presentMode the presentation mode to check
         *
         * \returns SDL_TRUE if supported, SDL_FALSE if unsupported (or on error)
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuSupportsPresentMode(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window,
            SDL_GpuPresentMode presentMode);

        /**
         * Claims a window, creating a swapchain structure for it.
         * This must be called before SDL_GpuAcquireSwapchainTexture is called using the window.
         *
         * This function will fail if the requested present mode or swapchain composition
         * are unsupported by the device. Check if the parameters are supported via
         * SDL_GpuSupportsPresentMode / SDL_GpuSupportsSwapchainComposition prior to
         * calling this function.
         *
         * SDL_GPU_PRESENTMODE_VSYNC and SDL_GPU_SWAPCHAINCOMPOSITION_SDR are
         * always supported.
         *
         * \param device a GPU context
         * \param window an SDL_Window
         * \param swapchainComposition the desired composition of the swapchain
         * \param presentMode the desired present mode for the swapchain
         *
         * \returns SDL_TRUE on success, otherwise SDL_FALSE.
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuAcquireSwapchainTexture
         * \sa SDL_GpuUnclaimWindow
         * \sa SDL_GpuSupportsPresentMode
         * \sa SDL_GpuSupportsSwapchainComposition
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuClaimWindow(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window,
            SDL_GpuSwapchainComposition swapchainComposition,
            SDL_GpuPresentMode presentMode);

        /**
         * Unclaims a window, destroying its swapchain structure.
         *
         * \param device a GPU context
         * \param window an SDL_Window that has been claimed
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuClaimWindow
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuUnclaimWindow(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window);

        /**
         * Changes the swapchain parameters for the given claimed window.
         *
         * This function will fail if the requested present mode or swapchain composition
         * are unsupported by the device. Check if the parameters are supported via
         * SDL_GpuSupportsPresentMode / SDL_GpuSupportsSwapchainComposition prior to
         * calling this function.
         *
         * SDL_GPU_PRESENTMODE_VSYNC and SDL_GPU_SWAPCHAINCOMPOSITION_SDR are
         * always supported.
         *
         * \param device a GPU context
         * \param window an SDL_Window that has been claimed
         * \param swapchainComposition the desired composition of the swapchain
         * \param presentMode the desired present mode for the swapchain
         * \returns SDL_TRUE if successful, SDL_FALSE on error
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSupportsPresentMode
         * \sa SDL_GpuSupportsSwapchainComposition
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuSetSwapchainParameters(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window,
            SDL_GpuSwapchainComposition swapchainComposition,
            SDL_GpuPresentMode presentMode);

        /**
         * Obtains the texture format of the swapchain for the given window.
         *
         * \param device a GPU context
         * \param window an SDL_Window that has been claimed
         *
         * \returns the texture format of the swapchain
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuTextureFormat SDL_GpuGetSwapchainTextureFormat(
            SDL_GpuDevice_Ptr device,
            SDL_Window_Ptr window);

        /**
         * Acquire a texture to use in presentation.
         * When a swapchain texture is acquired on a command buffer,
         * it will automatically be submitted for presentation when the command buffer is submitted.
         * The swapchain texture should only be referenced by the command buffer used to acquire it.
         * May return NULL under certain conditions. This is not necessarily an error.
         * This texture is managed by the implementation and must not be freed by the user.
         * You MUST NOT call this function from any thread other than the one that created the window.
         *
         * \param commandBuffer a command buffer
         * \param window a window that has been claimed
         * \param pWidth a pointer filled in with the swapchain width
         * \param pHeight a pointer filled in with the swapchain height
         * \returns a swapchain texture
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuClaimWindow
         * \sa SDL_GpuSubmit
         * \sa SDL_GpuSubmitAndAcquireFence
         */
        [DllImport(nativeLibName, EntryPoint = "SDL_GpuAcquireSwapchainTexture", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe SDL_GpuTexture_Ptr Inner_SDL_GpuAcquireSwapchainTexture(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            SDL_Window_Ptr window,
            UInt32* pWidth,
            UInt32* pHeight);

        public static (SDL_GpuTexture_Ptr Swapchain, UInt32 Width, UInt32 Height) SDL_GpuAcquireSwapchainTexture(
            SDL_GpuCommandBuffer_Ptr commandBuffer,
            SDL_Window_Ptr window)
        {
            unsafe
            {
                UInt32 Width;
                UInt32 Height;
                SDL_GpuTexture_Ptr Swapchain = Inner_SDL_GpuAcquireSwapchainTexture(commandBuffer, window, &Width, &Height);
                return (Swapchain, Width, Height);
            }
        }

        /**
         * Submits a command buffer so its commands can be processed on the GPU.
         * It is invalid to use the command buffer after this is called.
         *
         * \param commandBuffer a command buffer
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuAcquireCommandBuffer
         * \sa SDL_GpuAcquireSwapchainTexture
         * \sa SDL_GpuSubmitAndAcquireFence
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuSubmit(
            SDL_GpuCommandBuffer_Ptr commandBuffer);

        /**
         * Submits a command buffer so its commands can be processed on the GPU,
         * and acquires a fence associated with the command buffer.
         * You must release this fence when it is no longer needed or it will cause a leak.
         * It is invalid to use the command buffer after this is called.
         *
         * \param commandBuffer a command buffer
         * \returns a fence associated with the command buffer
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_AcquireCommandBuffer
         * \sa SDL_GpuAcquireSwapchainTexture
         * \sa SDL_GpuSubmit
         * \sa SDL_GpuReleaseFence
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuFence_Ptr SDL_GpuSubmitAndAcquireFence(
            SDL_GpuCommandBuffer_Ptr commandBuffer);

        /**
         * Blocks the thread until the GPU is completely idle.
         *
         * \param device a GPU context
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuWaitForFences
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuWait(
            SDL_GpuDevice_Ptr device);

        /**
         * Blocks the thread until the given fences are signaled.
         *
         * \param device a GPU context
         * \param waitAll if 0, wait for any fence to be signaled, if 1, wait for all fences to be signaled
         * \param pFences an array of fences to wait on
         * \param fenceCount the number of fences in the pFences array
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSubmitAndAcquireFence
         * \sa SDL_GpuWait
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SDL_GpuWaitForFences(
            SDL_GpuDevice_Ptr device,
            SDL_bool waitAll,
            SDL_GpuFence_Ptr* pFences,
            UInt32 fenceCount);

        /**
         * Checks the status of a fence.
         *
         * \param device a GPU context
         * \param fence a fence
         * \returns SDL_TRUE if the fence is signaled, SDL_FALSE if it is not
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSubmitAndAcquireFence
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuQueryFence(
            SDL_GpuDevice_Ptr device,
            SDL_GpuFence_Ptr fence);

        /**
         * Releases a fence obtained from SDL_GpuSubmitAndAcquireFence.
         *
         * \param device a GPU context
         * \param fence a fence
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSubmitAndAcquireFence
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GpuReleaseFence(
            SDL_GpuDevice_Ptr device,
            SDL_GpuFence_Ptr fence);

        /* Format Info */

        /**
         * Obtains the texel block size for a texture format.
         *
         * \param textureFormat the texture format you want to know the texel size of
         * \returns the texel block size of the texture format
         *
         * \since This function is available since SDL 3.x.x
         *
         * \sa SDL_GpuSetTransferData
         * \sa SDL_GpuUploadToTexture
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 SDL_GpuTextureFormatTexelBlockSize(
            SDL_GpuTextureFormat textureFormat);

        /**
         * Determines whether a texture format is supported for a given type and usage.
         *
         * \param device a GPU context
         * \param format the texture format to check
         * \param type the type of texture (2D, 3D, Cube)
         * \param usage a bitmask of all usage scenarios to check
         * \returns whether the texture format is supported for this type and usage
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_GpuIsTextureFormatSupported(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTextureFormat format,
            SDL_GpuTextureType type,
            SDL_GpuTextureUsageFlags usage);

        /**
         * Determines the "best" sample count for a texture format, i.e.
         * the highest supported sample count that is <= the desired sample count.
         *
         * \param device a GPU context
         * \param format the texture format to check
         * \param desiredSampleCount the sample count you want
         * \returns a hardware-specific version of min(preferred, possible)
         *
         * \since This function is available since SDL 3.x.x
         */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GpuSampleCount SDL_GpuGetBestSampleCount(
            SDL_GpuDevice_Ptr device,
            SDL_GpuTextureFormat format,
            SDL_GpuSampleCount desiredSampleCount);

        #endregion
    }
}
