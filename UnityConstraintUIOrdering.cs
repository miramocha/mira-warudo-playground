namespace Warudo.Plugins.Scene.Assets
{
    public static class UnityConstraintUIOrdering
    {
        public const int PARENT_SECTION = 9,
            PARENT_INPUT = 10,
            PARENT_TRANSFORM_PATH_INPUT = 11,
            SOURCE_SECTION = 119,
            SOURCE_INPUT = 120,
            SOURCE_TRANSFORM_PATH_INPUT = 121,
            // Constraint Settings
            SETTINGS_SECTION = 1000,
            CREATE_CONSTRAINT_TRIGGER = 1001,
            DELETE_CONSTRAINT_TRIGGER = 1002,
            LOCK_TRIGGER = 1002,
            WEIGHT_INPUT = 1003,
            POSITION_AT_REST_INPUT = 1004,
            ROTATION_AT_REST_INPUT = 1005,
            // Rotation constraint specific
            ROTATION_OFFSET_INPUT = 1021,
            RESET_ROTATION_OFFSET_TRIGGER = 1022,
            // Freeze Rotation
            FREEZE_ROTATION_SECTION = 1100,
            FREEZE_ROTATION_X_INPUT = 1101,
            FREEZE_ROTATION_Y_INPUT = 1102,
            FREEZE_ROTATION_Z_INPUT = 1103,
            // Freeze Position
            FREEZE_POSITION_SECTION = 1200,
            FREEZE_POSITION_X_INPUT = 1201,
            FREEZE_POSITION_Y_INPUT = 1202,
            FREEZE_POSITION_Z_INPUT = 1203,
            // Debug
            DEBUG_SECTION = 2000,
            DEBUG_MODE_INPUT = 2001,
            CLEAR_DEBUG_LOG_TRIGGER = 2002,
            // Debug Info
            DEBUG_INFO_SECTION = 2500,
            DEBUG_LOG_HEADER = 2501,
            DEBUG_LOG_MESSAGE = 2502;
    }
}
