namespace RemSend;

/*public readonly struct To {
    public ToType Type { get; }
    public int? PeerId { get; }
    public IEnumerable<int>? PeerIds { get; }

    private To(ToType Type, int? PeerId, IEnumerable<int>? PeerIds) {
        this.Type = Type;
        this.PeerId = PeerId;
        this.PeerIds = PeerIds;
    }

    public static To All => new(ToType.Broadcast, null, null);
    public static To Peer(int PeerId) => new(ToType.Single, PeerId, null);
    public static To Peers(IEnumerable<int> PeerIds) => new(ToType.Multiple, null, PeerIds);

    public static implicit operator To(int PeerId) => Peer(PeerId);
    public static implicit operator To(int[] PeerIds) => Peers(PeerIds);
    public static implicit operator To(List<int> PeerIds) => Peers(PeerIds);
}

public enum ToType {
    Broadcast,
    Single,
    Multiple,
}*/