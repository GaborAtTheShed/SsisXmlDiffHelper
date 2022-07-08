namespace SsisXmlDiffHelper.Models
{
    public class DataFlowComponent
    {
        public string? Name { get; set; }
        public string? ComponentId { get; set; }
        public SqlScriptProperty? SqlScriptProperty { get; set; }
        public IEnumerable<string?>? InputColumns { get; set; }
        public IEnumerable<string?>? OutputColumns { get; set; }
    }
}