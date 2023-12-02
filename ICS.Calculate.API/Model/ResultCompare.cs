namespace ICS.Calculate.API.Model;

public class ResultCompare
{
    public ResultCompare(){ }

    public List<ResultData> Series { get; set; }
}

public class ResultData
{
    public string Name { get; set; }
    public double Data { get; set; }
}

