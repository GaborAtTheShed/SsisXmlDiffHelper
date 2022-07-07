namespace SsisXmlDiffHelper.Models
{
    public class DataFlowComponent
    {
        public string? Name { get; set; }
        public string? ComponentId { get; set; }
        public SqlScriptProperty? SqlScriptProperty { get; set; }
    }
}