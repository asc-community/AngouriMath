//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The elementary functions at a hundred digits against reference values (mpmath at a
    /// hundred and twenty), to ninety-eight digits: the trigonometric functions are argument
    /// halving and short series now, the arctangent its own series and the arcsine the
    /// arctangent's, and each is checked where its predecessor lost digits -- the sine near
    /// zero, the arcsine near one, the arctangent far out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1338">#1338</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class HighPrecisionFunctionsTest
    {
        private static void AgreesTo98Digits(Entity expression, string reference)
        {
            var got = expression.EvalNumerical();
            Assert.True(got is Real, $"{expression} evaluated to {got}, not a real");
            var want = EDecimal.FromString(reference);
            var context = new EContext(110, ERounding.HalfUp, -200, 2000, false);
            var difference = ((Real)got).EDecimal.Subtract(want, context).Abs();
            var scale = want.Abs();
            var relative = difference.Divide(scale, context);
            Assert.True(relative.CompareTo(EDecimal.Create(1, -97)) < 0,
                $"{expression} is {got} and should be {reference}: relative error {relative}");
        }

        [Theory]
        [InlineData("sin(0.37)", "0.361615431964961978037292469127154274696845151425682771086194418360140139217306430899962671433495691370857")]
        [InlineData("sin(1.7)", "0.991664810452468615346133398647875652406819571167123725327102491023305035733742936829620236666377682012327")]
        [InlineData("sin(-2.9)", "-0.239249329213982328184256918739575372215552930299618774116210265880710778671232942508115049276324462128351")]
        [InlineData("sin(10.5)", "-0.879695759971670098018508684825996034694780506297492734472544018573425918530651444976401779597118657421823")]
        [InlineData("sin(3)", "0.141120008059867222100744802808110279846933264252265584151882641232422009967014471911282172853449863750414")]
        [InlineData("cos(0.37)", "0.932327345606034423203812904490877885689405282263576632805449177245576067326844337659367361595858588791846")]
        [InlineData("cos(1.7)", "-0.128844494295524684087642857334873514101640079645202976331782139942894356545082000333290557186582283470409")]
        [InlineData("cos(-2.9)", "-0.970958165149590521781106669345532179117614759424239542138670992453273283056746079014115234944011412994284")]
        [InlineData("cos(10.5)", "-0.475536927995992535523811490148858726078717741593479642557859450133182014859695562386622463538202509444257")]
        [InlineData("tan(0.37)", "0.387863161655849052224444566371827968609315309826965860174746100134967076947711249291126396972000086713446")]
        [InlineData("tan(1.7)", "-7.6966021394591584141281929682986609163652899143076475629457414231809781368970775977290877390625456458864")]
        [InlineData("arcsin(0.37)", "0.379009020695950814074873624644761613369247292477933770476829074410249291201750337690532903117708726084071")]
        [InlineData("arcsin(-0.5)", "-0.523598775598298873077107230546583814032861566562517636829157432051302734381034833104672470890352844663691")]
        [InlineData("arctan(0.37)", "0.354379919123437809830726351143989290433304513586222681812525656922795656839750947554131332616663954914309")]
        [InlineData("arctan(3.7)", "1.30683260316919205666262523215099008155312636607424143926882689069704014069633292419956191884840311712425")]
        [InlineData("arctan(-0.05)", "-0.0499583957219427614100062870348448814912770804235071744108534548299835954767103350612648887048501265496759")]
        [InlineData("arctan(1000000)", "1.57079532679489661956465502497288477543191817587802910085255166123336419159909287837939647811679057972306")]
        [InlineData("arccos(0.37)", "1.191787306098945805156448066994989828729337407209619140010643221743658911941354161623484509553349807907")]
        public void TheElementaryFunctionsAtAHundredDigits(string expression, string reference)
            => AgreesTo98Digits(expression.ToEntity(), reference);

        /// <summary>
        /// Where the old implementations lost digits: the sine as <c>sqrt(1 - cos^2)</c> kept
        /// fifty of a hundred near zero, the cosine a hair from a right angle, and the arcsine
        /// near one.
        /// </summary>
        [Theory]
        [InlineData("sin(0.0000000001)", "0.0000000000999999999999999999998333333333333333333334166666666666666666666468253968253968253968281525573192239858907")]
        [InlineData("cos(1.5707963)", "0.0000000267948966192313184853334619029956637318894971745136916953499296022105640375379488696852156030209360627392")]
        [InlineData("arcsin(0.999999)", "1.56938211311467236746824989586709579363455866391912675020764163786452703997753443323569614111361345619033")]
        public void WhereDigitsWereLost(string expression, string reference)
            => AgreesTo98Digits(expression.ToEntity(), reference);

        /// <summary>
        /// A whole power and a half power of a decimal, which are one call on the decimal now
        /// and were a chain of multiplications or an exponential of a logarithm.
        /// </summary>
        [Theory]
        [InlineData("1.7^3", "4.913")]
        [InlineData("1.7^(-3)", "0.203541624262161612049664156319967433340118054142072053734988805210665581111337268471402401791166293507022")]
        [InlineData("sqrt(1.7)", "1.30384048104052974291659431148583688330561875578201309179007936989676538557639789654518352888678849773386")]
        [InlineData("1.7^(3/2)", "2.21652881776890056295821032952592270161955188482942225604313492882450115547987642412681199910754044614756")]
        [InlineData("1.7^(-1/2)", "0.766964988847370437009761359697551107826834562224713583405929041115744344456704645026578546403993233961094")]
        public void WholeAndHalfPowersOfADecimal(string expression, string reference)
            => AgreesTo98Digits(expression.ToEntity(), reference);

        /// <summary>
        /// The logarithm, the exponential, a real power and the hyperbolic functions, which are
        /// series in fixed point now rather than PeterO's: near one and far from it, a power of
        /// ten and a power of e -- <c>e^700</c> came back to ninety-seven digits as the
        /// constant's hundred raised to the seven hundredth. Not below ten to the minus fifty:
        /// a value within half the working digits of an integer is that integer by the
        /// downcasting, so <c>e^(-123.456)</c>, which is ten to the minus fifty-four, is 0.
        /// </summary>
        [Theory]
        [InlineData("ln(0.37)", "-0.994252273343866923667887238337281251302125390089909788421743120742155472364343895072972876507027935902127")]
        [InlineData("ln(1.1369)", "0.128305260152923841561498035928794898057419676174790423366932255144985972701456882347699236277855162624175")]
        [InlineData("ln(0.9999)", "-0.000100005000333358335333500014286964396835397734571075514089865762716344756611402617022495986337466528754179")]
        [InlineData("ln(1.00001)", "0.00000999995000033333083335333316666809522559534920534921544003210755133040854707452208102937253322237289734812")]
        [InlineData("ln(750)", "6.62007320653035612461475535805926519129979475498855787159331801755342487831127697636988071608968960872987")]
        [InlineData("ln(0.000001234)", "-13.6052496324810780327471192920909104756199783466598383294738650174947053425106213063054082875112674176785")]
        [InlineData("ln(123456789.123456789)", "18.6314017671680180326939333482965375427970151745537353083517566119017412766551613015767513407252233304900")]
        [InlineData("log(10, 0.37)", "-0.431798275933005003191549310460870552017027309833687453382320089206414574578527530257797656739549285246099")]
        [InlineData("log(3, 81.5)", "4.00560148984118758502141372215507044442357868461710473567971265348027971343638223614214033083963425256794")]
        [InlineData("e^0.37", "1.44773461466332446158475233551922961456683194184845491456061206892295956926655134553108541144999797619663")]
        [InlineData("e^(-0.5)", "0.606530659712633423603799534991180453441918135487186955682892158735056519413748423998647611507989456026424")]
        [InlineData("e^25.5", "118716009132.169650965201023040233373526449091282754098342267442638097384414108824078431787894306908281050")]
        [InlineData("e^700", "1.01423205473500450945532959523126761520467957224307334878053628124935170250752368304548160316182971369539e+304")]
        [InlineData("e^(-100.5)", "2.25634013591703631320281828241534366417520785936464023567989659964288626428305623878961101522065173672292e-44")]
        [InlineData("e^0.00001234", "1.00001234007613811318111682981596300497517047048619342510199268785525707213639963880173472004809302592953")]
        [InlineData("2^0.37", "1.29235283063749224450556503197070707880867324808527754395903593457909430919166483018351136908382587001355")]
        [InlineData("10^(-0.37)", "0.426579518801592658004523896600975319499453929539193328227549626145447937991082374795110805295935878724416")]
        [InlineData("1.7^2.9", "4.65909828378606751689142946026153054833060144518731308290895183497457393121344828497028520145573250549740")]
        [InlineData("0.37^0.37", "0.692204850015734267986295538874892165757969134156257794701426036412631395756556597414370238390864299729380")]
        [InlineData("sinh(0.37)", "0.378500142012984901014906185639889080855936538762531408933184424856263739634050081675804991499874453185980")]
        [InlineData("cosh(-2.9)", "9.11458429474973408585310155248112110830188451116796598581181237851004903258039556059848985280824921641633")]
        [InlineData("tanh(0.37)", "0.353991712477045994713511975971816782189871283978107285505039728240177889286936341011158953779536534740937")]
        public void LogarithmsExponentialsAndPowers(string expression, string reference)
            => AgreesTo98Digits(expression.ToEntity(), reference);

        /// <summary>
        /// The downcasting to a rational is decided cheaply first and exactly after: what was
        /// a rational is still one, and what is not is not.
        /// </summary>
        [Theory]
        [InlineData("0.5", true)]
        [InlineData("2.07", true)]
        [InlineData("0.333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333", true)]
        [InlineData("3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798", false)]
        [InlineData("0.37000000000001234", false)]
        [InlineData("1234567.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001", true)]
        public void TheRationalSearchStillAgrees(string number, bool isRational)
        {
            var found = Rational.FindRational(EDecimal.FromString(number));
            Assert.Equal(isRational, found is not null);
        }

        /// <summary>
        /// The cheap decision against the exact search on twelve thousand rationals within the
        /// bound, on as many values a little off one -- within ten to the minus ten to ten to
        /// the minus fifteen, which is where the double can no longer tell and the
        /// double-double must -- and on as many random hundred-digit values: the filter never
        /// refuses a value the exact search accepts, and the two agree on every one.
        /// </summary>
        [Fact]
        public void TheCheapDecisionAgreesWithTheExactSearch()
        {
            var random = new System.Random(1338);
            var context = MathS.Settings.DecimalPrecisionContext.Value;
            var iterCount = MathS.Settings.FloatToRationalIterCount.Value;
            var bound = 100_000_000;
            var refused = 0;
            var checkedCount = 0;
            void Check(EDecimal value)
            {
                var exact = Rational.FindRationalExactly(value, iterCount);
                var cheap = Rational.MayBeASmallRational(value, iterCount);
                var found = Rational.FindRational(value);
                checkedCount++;
                if (exact is not null && !cheap)
                    refused++;
                Assert.Equal(exact is not null, found is not null);
                if (exact is not null)
                    Assert.Equal(exact, found);
            }
            for (var i = 0; i < 12000; i++)
            {
                var denominator = 1 + random.Next(bound);
                var numerator = random.Next(bound);
                var value = EDecimal.FromInt32(numerator).Divide(EDecimal.FromInt32(denominator), context);
                Check(value);
                // Off by ten to the minus ten to fifteen: the double loses the question there.
                var offset = EDecimal.FromDouble(random.NextDouble()).Multiply(EDecimal.Create(1, -10 - random.Next(6)), context);
                Check(value.Add(offset, context));
            }
            for (var i = 0; i < 12000; i++)
            {
                var digits = new System.Text.StringBuilder("0.");
                for (var d = 0; d < 100; d++)
                    digits.Append((char)('0' + random.Next(10)));
                Check(EDecimal.FromString(digits.ToString()).Multiply(EDecimal.FromInt32(1 + random.Next(1000)), context));
            }
            Assert.Equal(0, refused);
            Assert.Equal(36000, checkedCount);
        }

        /// <summary>
        /// A value the double cannot refuse -- sin 1, whose twelfth remainder is within the
        /// double's error of the next integer -- is refused by the double-double, and never
        /// reaches the exact search. https://github.com/asc-community/AngouriMath/issues/1338
        /// </summary>
        [Fact]
        public void TheDoubleDoubleRefusesWhatTheDoubleCannot()
        {
            var sin1 = EDecimal.FromString("0.8414709848078965066525023216302989996225630607983710656727517099919104043912396689486397435430526958");
            Assert.False(Rational.MayBeASmallRational(sin1, MathS.Settings.FloatToRationalIterCount));
            Assert.Null(Rational.FindRational(sin1));
        }

        /// <summary>
        /// The cheap decision reads a value of two thousand digits too: two halves of its
        /// binary shift were each past a double's range, and every such value went to the
        /// exact search. The decision and the search still agree there, on rationals and on
        /// random digits. https://github.com/asc-community/AngouriMath/issues/1338
        /// </summary>
        [Fact]
        public void TheCheapDecisionReadsTwoThousandDigits()
        {
            var context = new EContext(2000, ERounding.HalfUp, -5000, 5000, false);
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(context);
            var iterCount = MathS.Settings.FloatToRationalIterCount.Value;
            var random = new System.Random(2000);
            var readable = 0;
            for (var i = 0; i < 200; i++)
            {
                var denominator = 1 + random.Next(100_000_000);
                var numerator = random.Next(100_000_000);
                var value = EDecimal.FromInt32(numerator).Divide(EDecimal.FromInt32(denominator), context);
                var exact = Rational.FindRationalExactly(value, iterCount);
                if (exact is not null)
                    Assert.True(Rational.MayBeASmallRational(value, iterCount));
                Assert.Equal(exact, Rational.FindRational(value));
                var digits = new System.Text.StringBuilder("0.");
                for (var d = 0; d < 2000; d++)
                    digits.Append((char)('0' + random.Next(10)));
                var noise = EDecimal.FromString(digits.ToString());
                if (!Rational.MayBeASmallRational(noise, iterCount))
                    readable++;
                Assert.Null(Rational.FindRational(noise));
            }
            // Random digits are refused by the cheap decision almost always; a reading
            // that could not cover the width would refuse none.
            Assert.True(readable > 150, $"{readable} of 200 refused cheaply");
        }

        private static readonly EContext threeHundredDigits = new(300, ERounding.HalfUp, -5000, 5000, false);

        private static int DigitsOf(Entity number) => number.ToString().TrimStart('-').Replace(".", "").Length;

        /// <summary>
        /// A constant is worth the current precision's digits, whichever precision it was
        /// first evaluated at -- pi at a hundred digits, then three hundred, then a hundred
        /// again; and the same object of the same expression, held across a change of the
        /// precision, is evaluated afresh at the new one rather than answered from its cache.
        /// https://github.com/asc-community/AngouriMath/issues/1367
        /// </summary>
        [Fact]
        public void ConstantsAndCachedEvaluationsFollowThePrecision()
        {
            var pi = MathS.pi;
            var held = MathS.Sin(1) + MathS.pi;
            Assert.Equal(100, DigitsOf(pi.EvalNumerical()));
            Assert.Equal(100, DigitsOf(held.EvalNumerical()));
            using (MathS.Settings.DecimalPrecisionContext.Set(threeHundredDigits))
            {
                Assert.Equal(300, DigitsOf(pi.EvalNumerical()));
                Assert.Equal(300, DigitsOf(held.EvalNumerical()));
                Assert.Equal(300, DigitsOf(MathS.FromString("pi").EvalNumerical()));
                Assert.StartsWith("3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127", pi.EvalNumerical().ToString());
            }
            Assert.Equal(100, DigitsOf(pi.EvalNumerical()));
            Assert.Equal(100, DigitsOf(held.EvalNumerical()));
        }

        /// <summary>
        /// <c>e</c> evaluated with the downcasting off used to be a complex number with a zero
        /// imaginary part, and the cache on the constant kept it that way for the rest of the
        /// process -- after which <c>sgn(e^x - 1)</c> was not real-valued and its derivative,
        /// which is zero, stayed unevaluated. A constant is a real number whatever the
        /// downcasting says, and a complex number with a zero imaginary part is real-valued
        /// in any case. https://github.com/asc-community/AngouriMath/issues/1367
        /// </summary>
        [Fact]
        public void AConstantIsRealWhateverTheDowncasting()
        {
            var sign = MathS.FromString("sgn((e^(2*x) - 1)/(e^(2*x) + 1))");
            using (MathS.Settings.DowncastingEnabled.Set(false))
            {
                Assert.IsType<Real>(MathS.e.Evaled);
                Assert.IsType<Real>(MathS.pi.Evaled);
                Assert.DoesNotContain("derivative(", MathS.FromString("sgn(e^x - 1)").Differentiate("x").ToString());
                Assert.DoesNotContain("derivative(", MathS.Signum(Complex.Create(EDecimal.FromString("1.5"), EDecimal.Zero) * MathS.Var("x")).Differentiate("x").ToString());
            }
            Assert.IsType<Real>(MathS.e.Evaled);
            Assert.DoesNotContain("derivative(", sign.Differentiate("x").ToString());
        }

        /// <summary>
        /// e to a power at three hundred digits, after the same expression's shape was
        /// evaluated at a hundred: it used to come back correct to a hundred, since the base
        /// was the hundred-digit e and no longer the exponential's own constant.
        /// https://github.com/asc-community/AngouriMath/issues/1367
        /// </summary>
        [Fact]
        public void EToAPowerAtAHigherPrecisionAfterALowerOne()
        {
            var x = Real.Create(EDecimal.FromString("0.37000000000001234"));
            Assert.Equal(100, DigitsOf(MathS.Pow(MathS.e, x).EvalNumerical()));
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(threeHundredDigits);
            Assert.StartsWith("1.4477346146633423266298972810533128989549659677348909712480333771187271973782808968904998312256362611570708175918866347464490548332052196352156710435710332682587883217473960307622422354384764392652386915303661697273096380952633917033052762392317525332683262721013424903315670646133049105751036966", MathS.Pow(MathS.e, x).EvalNumerical().ToString());
        }
    }
}
