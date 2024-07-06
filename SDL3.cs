
// The contents of this file is largely kitbashed from these two repositories,
// and their respective copyrights and licenses probably also apply here:
//  - https://github.com/flibitijibibo/SDL2-CS/
//  - https://github.com/thatcosmonaut/SDL/tree/gpu/include/SDL3

using System;
using System.Runtime.InteropServices;
using System.Text;

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
            public UInt32 type;        /**< Event type, shared with all events, Uint32 to cover user events which are not in the SDL_EventType enumeration */
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
         * The structure for all events in SDL.
         *
         * \since This struct is available since SDL 3.0.0.
         */
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct SDL_Event
        {
            [FieldOffset(0)]
            public SDL_EventType type;                            /**< Event type, shared with all events, Uint32 to cover user events which are not in the SDL_EventType enumeration */

            [FieldOffset(0)]
            public UInt32 type_uint;                            /**< Event type, shared with all events, Uint32 to cover user events which are not in the SDL_EventType enumeration */

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
            SDL_GamepadDeviceEvent gdevice;         /**< Gamepad device event data */
            SDL_GamepadAxisEvent gaxis;             /**< Gamepad axis event data */
            SDL_GamepadButtonEvent gbutton;         /**< Gamepad button event data */
            SDL_GamepadTouchpadEvent gtouchpad;     /**< Gamepad touchpad event data */
            SDL_GamepadSensorEvent gsensor;         /**< Gamepad sensor event data */
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

        #region SDL_video
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
        private static extern unsafe IntPtr Inner_SDL_CreateWindow(byte* title, int w, int h, UInt64 flags);

        // Pass in title strings like so: SDL_CreateWindow("My Game Title"u8, etc, etc)
        public static unsafe IntPtr SDL_CreateWindow(ReadOnlySpan<byte> title, int w, int h, UInt64 flags)
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
        public static extern void SDL_DestroyWindow(IntPtr window);
        #endregion

        #region SDL_gpu.h
        #endregion
    }
}
