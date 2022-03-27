using SMapper;
using Newtonsoft.Json;
using System.Reflection;

var a = new A()
{
    display_name = "Alice",
    id = 321,
    forConvert = "i123"
};
Console.WriteLine("origin");
Console.WriteLine(JsonConvert.SerializeObject(a));
Console.WriteLine();

//simple usage
var b = Mapper<B>.Convert(a);
Console.WriteLine("simple");
Console.WriteLine(JsonConvert.SerializeObject(b));
Console.WriteLine();
//converter usage
Console.WriteLine("converter");
var c = Mapper<C>.Convert(a, MapperSettings.Default, new StringToIntConverter());
Console.WriteLine(JsonConvert.SerializeObject(c));
Console.WriteLine();

//attribute and setting usage
Console.WriteLine("attribute and mapperSettings");
var d = Mapper<D>.Convert(a, new MapperSettings
{
    IgnoreUndercase = false,
    ToLowerCase = false,
}, new StringToIntConverter());
Console.WriteLine(JsonConvert.SerializeObject(d));
Console.WriteLine();

//dynamic
Console.WriteLine("dynamic class creation");
var e = Mapper.Convert(a);
Console.WriteLine(JsonConvert.SerializeObject(e));
Console.WriteLine();

public class A
{
    public string display_name { get; set; }
    public int id { get; set; }    
    public string forConvert { get; set; }
}

public class B
{
    public string DisplayName { get; set; }
    public int Id { get; set; }   
}

public class C
{
    public string DisplayName { get; set; }
    public int Id { get; set; }
    public int ForConvert { get; set; }
}

public class D
{
    [MapperIgnore]
    public string display_name { get; set; } = "default value";

    public int id { get; set; }

    public int Id { get; set; } = 777;

    [MapperAlias("forConvert")]
    public int Value { get; set; }
}

public class StringToIntConverter : MapperConverter<string, int>
{
    public override int Convert(string origin)
    {
        return int.Parse(origin.Trim('i'));
    }
}