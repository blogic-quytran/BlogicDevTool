namespace BLogicDevTool;

public class SqlLoginInfo
{
    public string Name        { get; set; } = "";
    public string Type        { get; set; } = "";
    public bool   IsDisabled  { get; set; }
    public bool   IsSysAdmin  { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
}

public class SqlServerInfo
{
    public string ServerName  { get; set; } = "";
    public string Version     { get; set; } = "";
    public string Edition     { get; set; } = "";
    public string MachineName { get; set; } = "";
    public bool   IsMixedMode { get; set; }
}
