namespace TypesElma;

public class WebData
{
    public List<WebDataItem> Items { get; set; }
    public object Value { get; set; }
}

public class WebDataItem
{
    public WebData Data { get; set; }
    public List<WebData> DataArray { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

}