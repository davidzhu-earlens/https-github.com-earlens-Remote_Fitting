namespace AwesomeCommon
{
    public enum CommandType
    {
        UNDEFINED = 0,
    	CREATE_CHATROOM = 101,
        DELETE_CHATROOM,
        JOIN_CHATROOM,
        ADD_TO_CHATROOM,
        REMOVE_FROM_CHATROOM,
        SEND_MESSAGE = 106,
        SETUP_CLIENT,

        PLAY_TONE = 500
    }
}
