using LanguageExt.Traits;
using Xunit;

namespace common.tests;

public class CommonTests
{
    [Fact]
    public void JsonResult_passes_monad_laws() =>
        MonadLaw<JsonResult>.assert();

    [Fact]
    public void JsonResult_passes_choice_laws() =>
        ChoiceLaw<JsonResult>.assert(JsonResult.Fail<int>(JsonError.From("Some error")));

}