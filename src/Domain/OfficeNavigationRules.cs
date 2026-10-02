namespace OpenGDS2.Domain;

/// <summary>Original fourteen-node routes and isometric movement, Module2 11.</summary>
public static class OfficeNavigationRules
{
    public static int DestinationNode(int action)=>action switch
    {
        1 or 2=>1,3 or 4 or 6 or 7=>2,5=>6,8=>7,9 or 14=>8,
        10 or 11=>10,12 or 13=>4,15=>9,16 or 17=>11,18=>12,19=>13,20=>14,
        _=>throw new ArgumentOutOfRangeException(nameof(action))
    };
    public static (int X,int Y) NextWaypoint(int node,int destination)
    {
        Node(node);Node(destination);
        return node switch
        {
            1=>(43,41),
            2=>destination is 1 or 7 or 10 ? (43,41):(55,53),
            3=>destination is 1 or 2 or 6 or 7 or 8 or 10 ? (55,53):(57,55),
            4=>destination is 1 or 2 or 3 or 6 or 7 or 8 or 10 or 12 or 13 ? (57,55):(63,61),
            5=>destination is 1 or 2 or 3 or 4 or 6 or 7 or 8 or 10 or 12 or 13 or 14 ? (63,61):(66,64),
            6=>(55,53),
            7=>destination is 1 or 2 or 3 or 4 or 5 or 6 or 8 or 9 or 12 or 13 or 14 ? (43,41):(31,53),
            8=>destination is 1 or 2 or 3 or 4 or 5 or 6 or 7 or 9 or 12 or 13 or 14 ? (55,53):(43,65),
            9=>destination is 10 or 11 or 8 ? (54,76):(66,64),
            10=>destination is 1 or 7 ? (31,53):(43,65),
            11=>destination is 9 or 4 or 5 or 12 or 13 or 14 ? (54,76):(43,65),
            12=>destination is 13 or 14 ? (63,49):(57,55),
            13=>destination is 14 or 4 or 5 or 9 or 11 ? (69,55):(63,49),
            _=>destination is 12 or 13 ? (69,55):(63,61)
        };
    }
    public static int? NodeOnArrival((int X,int Y) point,int destination)
    {
        Node(destination);
        return point switch
        {
            (55,53)=>destination switch {1 or 7=>2,10=>8,12 or 13 or 11 or 14 or 3 or 4 or 5 or 9=>3,8 or 2 or 6=>destination,_=>null},
            (57,55)=>destination switch {1 or 2 or 3 or 8 or 10 or 7 or 6=>3,5 or 9 or 11 or 14=>4,12 or 13=>12,4=>destination,_=>null},
            (63,61)=>destination switch {1 or 2 or 3 or 8 or 7 or 10 or 6 or 12=>4,13 or 14=>14,5 or 9 or 11=>5,4=>destination,_=>null},
            (43,41)=>destination switch {1 or 2 or 7=>destination,10=>7,_=>2},
            (43,65)=>destination switch {10 or 8 or 11=>destination,1 or 2 or 3 or 4 or 5 or 6 or 12 or 13 or 14=>8,7=>10,9=>11,_=>null},
            (54,76)=>destination switch {11 or 9=>destination,10 or 8 or 7 or 1 or 2 or 6=>11,3 or 4 or 5 or 12 or 13 or 14=>9,_=>null},
            (31,53)=>destination switch {7 or 10=>destination,1 or 2 or 6 or 3 or 4 or 5 or 12 or 13 or 14=>7,8 or 11 or 9=>10,_=>null},
            (66,64)=>destination switch {4 or 5 or 1 or 2 or 3 or 6 or 7 or 8 or 10 or 12 or 13 or 14=>5,9=>destination,11=>9,_=>null},
            (63,49)=>destination<=11 ? 12 : destination==12 ? destination : 13,
            (69,55)=>destination<=11 ? 14 : destination==12 ? 13 : destination,
            _=>null
        };
    }
    public static (int X,int Y) WorkTarget(int action)=>action switch
    {
        1=>(37,35),2=>(41,39),3=>(45,43),4=>(49,47),5=>(58,46),6=>(47,45),
        7=>(51,49),8=>(38,46),9=>(48,60),10=>(35,57),11=>(39,61),12=>(59,57),
        13=>(63,61),14=>(50,58),15=>(60,72),16=>(47,69),17=>(51,73),
        _=>throw new ArgumentOutOfRangeException(nameof(action))
    };
    public static int WorkFacing(int action)=>action switch
    {
        >=1 and <=4 or 10 or 11 or 16 or 17=>2,5 or 9 or 15=>1,
        6 or 7 or 12 or 13=>4,8 or 14=>3,_=>throw new ArgumentOutOfRangeException(nameof(action))
    };
    public static int Direction(double x,double y,double targetX,double targetY)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(targetX) || !double.IsFinite(targetY))
            throw new ArgumentOutOfRangeException(nameof(x));
        if (x==targetX && y==targetY) return 0;
        if (x<=targetX && y<=targetY) return targetX-targetY==x-y ? 3 : targetX-targetY>=x-y ? 2 : 4;
        if (x>targetX && y<targetY) return targetX+targetY==x+y ? 4 : targetX+targetY<=x+y ? 1 : 3;
        if (x>=targetX && y>=targetY) return targetX-targetY==x-y ? 1 : targetX-targetY<=x-y ? 4 : 2;
        return targetX+targetY==x+y ? 2 : targetX+targetY>=x+y ? 3 : 1;
    }
    public static void Step(OfficeEmployeeState office,(int X,int Y) target)
    {
        if (office.X%4!=0 || office.Y%2!=0) throw new InvalidDataException("Employee is off the original half-grid.");
        int direction=Direction(office.X/8.0+20,office.Y/4.0+20,target.X,target.Y);
        office.MovementDirection=direction;
        if (direction==0) return;
        office.X+=direction is 1 or 4 ? -4 : 4;
        office.Y+=direction is 1 or 2 ? -2 : 2;
        office.WalkingTicks++;
        int phase=office.WalkingTicks/2;
        if (office.WalkingTicks%2==0)
        {
            office.Frame=(direction-1)*3+(phase==2 ? 2 : phase==4 ? 3 : 1);
            if (phase==4) office.WalkingTicks=0;
        }
    }
    private static void Node(int node) {if (node is <1 or >14) throw new ArgumentOutOfRangeException(nameof(node));}
}
