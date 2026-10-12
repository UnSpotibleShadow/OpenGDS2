namespace OpenGDS2.Domain;

/// <summary>fGMCrossR 009CAFCC: persisted comments and reveal callbacks.
/// Cosmetic score cycling/native whole-session RNG order remain separate.</summary>
public static class ReviewPresentationRules
{
    public static IReadOnlyList<int> CommentCounts {get;}=new[]{9,8,11,9,8,4};
    public static int Band(int score)=>score switch
    {>=1 and <=2=>0,<=4 and >=3=>1,<=6 and >=5=>2,<=8 and >=7=>3,9=>4,10=>5,_=>throw new ArgumentOutOfRangeException(nameof(score))};
    public static int? Reaction(int score)=>Band(score) switch {0=>4,1=>8,2=>9,3=>6,4=>null,_=>7};
    public static void Prepare(ReleasedGameState product,Random random)
    {
        if(product.ReviewCommentVariants is not null || product.ReviewScores is not {Count:4} scores)
            throw new InvalidOperationException("Review comments require fresh prepared scores.");
        var variants=new List<int>();
        foreach(int score in scores)
        {
            float draw=random.NextSingle();
            if(!float.IsFinite(draw) || draw<0 || draw>=1)throw new InvalidDataException("Invalid review comment draw.");
            variants.Add((int)Math.Floor(draw*CommentCounts[Band(score)]));
        }
        product.ReviewCommentVariants=variants;product.ReviewDisplayCallbacks=0;
    }
    public static bool Advance(ReleasedGameState product)
    {
        if(product.ReviewDisplayCallbacks is not int callbacks || callbacks>=90)return false;
        product.ReviewDisplayCallbacks=callbacks+1;return true;
    }
    public static bool Finished(ReleasedGameState product)=>product.ReviewDisplayCallbacks is null or >=90;
    public static bool Previewed(ReleasedGameState product,int row)=>
        row is >=0 and <4 ? product.ReviewDisplayCallbacks is null || product.ReviewDisplayCallbacks>=10+20*row :
        throw new ArgumentOutOfRangeException(nameof(row));
    public static bool Revealed(ReleasedGameState product,int row)=>
        row is >=0 and <4 ? product.ReviewDisplayCallbacks is null || product.ReviewDisplayCallbacks>=20+20*row :
        throw new ArgumentOutOfRangeException(nameof(row));
    public static void Validate(ReleasedGameState product)
    {
        if(product.ReviewDisplayCallbacks is <0 or >90 ||
            product.ReviewCommentVariants is { } variants &&
            (product.ReviewScores is not {Count:4} || product.ReviewScores.Any(s=>s is <1 or >10) || variants.Count!=4 || product.ReviewDisplayCallbacks is null ||
             variants.Where((v,i)=>v<0 || v>=CommentCounts[Band(product.ReviewScores![i])]).Any()) ||
            product.ReviewDisplayCallbacks is not null && product.ReviewScores is null)
            throw new InvalidDataException("Invalid saved review presentation.");
    }
}
