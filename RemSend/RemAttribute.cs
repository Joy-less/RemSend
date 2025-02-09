namespace RemSend;

/// <param name="Access"><inheritdoc cref="Access" path="/summary"/></param>
/// <param name="CallLocal"><inheritdoc cref="CallLocal" path="/summary"/></param>
/// <param name="Mode"><inheritdoc cref="Mode" path="/summary"/></param>
/// <param name="Channel"><inheritdoc cref="Channel" path="/summary"/></param>
[AttributeUsage(AttributeTargets.Method)]
public class RemAttribute(RemAccess Access = RemAccess.None, bool CallLocal = false, RemMode Mode = RemMode.Reliable, int Channel = 0) : Attribute {
    /// <summary>
    /// The access permissions for the remote method.<br/>
    /// Default: <see cref="RemAccess.None"/>
    /// </summary>
    public RemAccess Access { get; set; } = Access;
    /// <summary>
    /// If <see langword="true"/>, the remote method is called on the local peer when calling on all peers.<br/>
    /// Default: <see langword="false"/>
    /// </summary>
    public bool CallLocal { get; set; } = CallLocal;
    /// <summary>
    /// The send mode for remote method calls.<br/>
    /// Default: <see cref="RemMode.Reliable"/>
    /// </summary>
    public RemMode Mode { get; set; } = Mode;
    /// <summary>
    /// The transfer channel number for remote method calls.<br/>
    /// Changing this can reduce congestion between unrelated calls (e.g. chatting and attacking).<br/>
    /// Default: 0
    /// </summary>
    public int Channel { get; set; } = Channel;
}