using Steamworks;

public class PingResponseHandler : ISteamMatchmakingPingResponse
{
	public PingResponseHandler(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond) : base(onServerResponded, onServerFailedToRespond)
	{
	}

}
