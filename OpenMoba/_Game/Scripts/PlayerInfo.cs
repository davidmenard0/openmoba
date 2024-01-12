public class PlayerInfo
{
    public string Name;
    public int PeerID;
    public int Team;
    public int Resources = 0;

    public PlayerInfo Copy()
    {
        return new PlayerInfo{
            Name = this.Name,
            PeerID = this.PeerID,
            Team = this.Team
        };
    }
}