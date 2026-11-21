using Modeling_And_Simulation_EAS.Entities;
using Modeling_And_Simulation_EAS.Random;
using Modeling_And_Simulation_EAS.Simulation;

namespace Modeling_And_Simulation_EAS;

public class Store
{
    public string name;
    /// <summary>Seed used when no seed is given to the constructor.</summary>
    public const ulong DefaultSeed = 123456;

    private LCG rng = new LCG(DefaultSeed);
    
    public DiscreteDistribution DemandA { get; private set; }
    public DiscreteDistribution DemandB { get; private set; }
    public DiscreteDistribution LeadTime { get; private set; }
    
    private readonly LCG _demandARng;
    private readonly LCG _demandBRng;
    private readonly LCG _leadTimeRng;
    
    public bool KeepTable { get; set; } = true;

    public IReadOnlyList<DayRecord> Days => _days;
    public IReadOnlyList<Order> Orders => _orders;
    
    private readonly List<DayRecord> _days = new();
    private readonly List<Order> _orders = new();
    private readonly List<Order> _pending = new(); 
    
    private StoreParameters _parameters = new();
    private Product _productA = null!;
    private Product _productB = null!;
    
    private long _sumEndA, _sumEndB, _sumSpace, _totalProfit, _totalLostProfit;
    private long _demandTotalA, _demandTotalB, _soldTotalA, _soldTotalB;
    private int _stockoutDaysA, _stockoutDaysB, _trimmedDeliveries, _maxSpace;
    
    public Store(string name) : this(name, DefaultSeed) { }

    /// <summary>Creates a Store whose random numbers start from the given seed.</summary>
    public Store(string name, ulong seed)
    {
        this.name = name;
        _demandARng = new LCG(DeriveSeed(seed, 1));
        _demandBRng = new LCG(DeriveSeed(seed, 2));
        _leadTimeRng = new LCG(DeriveSeed(seed, 3));
    }
    
    public void SetDistributionTables(DiscreteDistribution DemandA, DiscreteDistribution DemandB,
        DiscreteDistribution LeadTime)
    {
        this.DemandA = DemandA;
        this.DemandB = DemandB;
        this.LeadTime = LeadTime;
    }
    
    public void BuildDistributionTable()
    {
        DemandA = new DiscreteDistribution
        ([5,10, 15, 20,25], [0.05, 0.1,0.25,0.3,0.3]);
        
        DemandB = new DiscreteDistribution
        ([3, 4, 5], [0.50, 0.35, 0.15]);
        
        LeadTime = new DiscreteDistribution
        ([1,2,3], [0.4, 0.35, 0.25]);
    }
    
    public RunResult Run(StoreParameters parameters)
    {
        parameters.Validate();

        Reset(parameters);

        for (int day = 1; day <= parameters.Days; day++)
        {
            var row = new DayRecord(day, parameters.ReviewPeriod);

            ReceiveDeliveries(day, row);                     // 1. morning
            ServeDemand(row);                                // 2. during the day
            CloseDay(row);                                   // 3. closing time
            if (day % parameters.ReviewPeriod == 0)
                ReviewStock(day, row);                       // 4. review every N days

            row.DaysUntilArrival = _pending.Count == 0 ? null : _pending.Min(o => o.ArrivalDay) - day;

            AddToTotals(row);
            if (KeepTable)
                _days.Add(row);
        }

        return ComputeResult();
    }
    
    private (int acceptedA, int acceptedB) AcceptDelivery(Order order)
    {
        int free = Math.Max(0, _parameters.Capacity - (_productA.SpaceUsed + _productB.SpaceUsed));
        int needed = order.QuantityA * _productA.Size + order.QuantityB * _productB.Size;

        if (needed <= free)
            return (order.QuantityA, order.QuantityB);

        int acceptedB = (int)((long)order.QuantityB * free / needed);
        int acceptedA = Math.Min(order.QuantityA, (free - acceptedB * _productB.Size) / _productA.Size);
        return (acceptedA, acceptedB);
    }
    
    private void ReceiveDeliveries(int day, DayRecord row)
    {
        // Oldest order first; with N ≤ 2 more than one order can arrive on the same day
        foreach (Order order in _pending.Where(o => o.ArrivalDay == day).ToList())
        {
            (int acceptedA, int acceptedB) = AcceptDelivery(order);

            _productA.Receive(order.QuantityA, acceptedA);
            _productB.Receive(order.QuantityB, acceptedB);
            order.MarkReceived(acceptedA, acceptedB);
            _pending.Remove(order);

            row.A.Received += acceptedA;
            row.B.Received += acceptedB;
            if (order.WasTrimmed)
                _trimmedDeliveries++;
        }

        row.A.Begin = _productA.OnHand;
        row.B.Begin = _productB.OnHand;
        _maxSpace = Math.Max(_maxSpace, _productA.SpaceUsed + _productB.SpaceUsed);
    }
    
    private void ServeDemand(DayRecord row)
    {
        ServeProduct(_productA, row.A, _demandARng);
        ServeProduct(_productB, row.B, _demandBRng);

        row.Profit = row.A.Sold * _productA.ProfitPerUnit + row.B.Sold * _productB.ProfitPerUnit;
        row.LostProfit = row.A.Lost * _productA.ProfitPerUnit + row.B.Lost * _productB.ProfitPerUnit;
    }
    
    private static void ServeProduct(Product product, ProductDay part, LCG rng)
    {
        part.DemandRn = NextRn(rng);
        part.Demand = product.Demand.Sample(part.DemandRn);
        part.Sold = product.Sell(part.Demand);
    }
    
    private void CloseDay(DayRecord row)
    {
        row.A.End = _productA.OnHand;
        row.B.End = _productB.OnHand;
        row.SpaceUsed = _productA.SpaceUsed + _productB.SpaceUsed;
    }
    
    private void ReviewStock(int day, DayRecord row)
    {
        int quantityA = _productA.QuantityToOrder();
        int quantityB = _productB.QuantityToOrder();
        if (quantityA + quantityB == 0)
            return;

        int rn = NextRn(_leadTimeRng);
        int leadTime = LeadTime.Sample(rn);
        var order = new Order(_orders.Count + 1, day, quantityA, quantityB, rn, leadTime);

        _productA.PlaceOrder(quantityA);
        _productB.PlaceOrder(quantityB);
        _orders.Add(order);
        _pending.Add(order);
        row.OrderPlaced = order;
    }
    
    private void AddToTotals(DayRecord row)
    {
        _sumEndA += row.A.End;
        _sumEndB += row.B.End;
        _sumSpace += row.SpaceUsed;
        _totalProfit += row.Profit;
        _totalLostProfit += row.LostProfit;
        _demandTotalA += row.A.Demand;
        _demandTotalB += row.B.Demand;
        _soldTotalA += row.A.Sold;
        _soldTotalB += row.B.Sold;
        if (row.A.StockedOut) _stockoutDaysA++;
        if (row.B.StockedOut) _stockoutDaysB++;
    }
    
    private RunResult ComputeResult()
    {
        double days = _parameters.Days;
        double dailyProfit = _totalProfit / days;

        return new RunResult(
            Days: _parameters.Days,
            AverageEndA: _sumEndA / days,
            AverageEndB: _sumEndB / days,
            AverageSpaceUsed: _sumSpace / days,
            MaxSpaceUsed: _maxSpace,
            TotalProfit: _totalProfit,
            AverageDailyProfit: dailyProfit,
            AverageCycleProfit: dailyProfit * _parameters.ReviewPeriod,
            LostUnitsA: _demandTotalA - _soldTotalA,
            LostUnitsB: _demandTotalB - _soldTotalB,
            TotalLostProfit: _totalLostProfit,
            StockoutDaysA: _stockoutDaysA,
            StockoutDaysB: _stockoutDaysB,
            FillRateA: _demandTotalA == 0 ? 1 : (double)_soldTotalA / _demandTotalA,
            FillRateB: _demandTotalB == 0 ? 1 : (double)_soldTotalB / _demandTotalB,
            AverageDemandA: _demandTotalA / days,
            AverageDemandB: _demandTotalB / days,
            AverageLeadTime: _orders.Count == 0 ? 0 : _orders.Average(o => o.LeadTime),
            OrderCount: _orders.Count,
            TrimmedDeliveries: _trimmedDeliveries);
    }
    
    private void Reset(StoreParameters parameters)
    {
        _parameters = parameters;
        _days.Clear();
        _orders.Clear();
        _pending.Clear();

        _productA = new Product("A", parameters.SizeA, parameters.ProfitA, parameters.LevelA, DemandA, parameters.InitialA);
        _productB = new Product("B", parameters.SizeB, parameters.ProfitB, parameters.LevelB, DemandB, parameters.InitialB);

        _sumEndA = _sumEndB = _sumSpace = _totalProfit = _totalLostProfit = 0;
        _demandTotalA = _demandTotalB = _soldTotalA = _soldTotalB = 0;
        _stockoutDaysA = _stockoutDaysB = _trimmedDeliveries = 0;
        _maxSpace = parameters.InitialSpace;
    }
    
    /// <summary>Random number from 1 to 100, used with the RN ranges of the tables.</summary>
    private static int NextRn(LCG rng) => rng.NextInt(1, 100);

    private static ulong DeriveSeed(ulong seed, ulong stream)
    {
        ulong z = seed + stream * 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        z ^= z >> 31;
        return z % 2147483646UL + 1;   // between 1 and 2³¹ − 2, never 0
    }
}