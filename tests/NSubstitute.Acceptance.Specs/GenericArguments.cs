using System.Collections;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;

namespace NSubstitute.Acceptance.Specs;

[TestFixture]
public class GenericArguments
{
    public interface ISomethingWithGenerics
    {
        void SomeAction<TState>(int level, TState state);
        string SomeFunction<TState>(int level, TState state);
        ICollection<TState> SomeFunction<TState>(TState state);
        ICollection<TState> SomeFunctionWithoutGenericMethodParameter<TState>(int level);
        bool SomeFunctionWithOut<TState>(out IEnumerable<TState> state);
        bool SomeFunctionWithRef<TState>(ref IEnumerable<TState> state);
        void SomeActionWithGenericConstraints<TState>(int level, TState state) where TState : IEnumerable<int>;
        string SomeFunctionWithGenericConstraints<TState>(int level, TState state) where TState : IEnumerable<int>;

        void SomeActionWithCovariant<TState>(ICovariant<TState> state);
        void SomeActionWithContravariant<TState>(IContravariant<TState> state);
        void SomeActionWithInvariant<TState>(IInvariant<TState> state);
        void SomeActionWithUnusedGenericArgument<TState>();

        TState SomeFunctionReturningGenericArgument<TState>();
        ICovariant<TState> SomeFunctionReturningCovariant<TState>();
        IContravariant<TState> SomeFunctionReturningContravariant<TState>();

        void SomeActionWithObject(object state);
        void SomeActionWithCovariantOfAnimal(ICovariant<Animal> state);
        void SomeActionWithContravariantOfCat(IContravariant<Cat> state);
        void SomeActionWithInvariantOfAnimal(IInvariant<Animal> state);
    }

    public abstract class MyAnyType : IEnumerable<int>, Arg.AnyType
    {
        public abstract IEnumerator<int> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Animal { }
    public class Cat : Animal { }
    public class Dog : Animal { }

    public interface ICovariant<out T> { T Get(); }
    public interface IContravariant<in T> { void Use(T value); }
    public interface IInvariant<T> { T Get(); void Use(T value); }

    [Test]
    public void Any_matcher_works_with_AnyType()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeAction(7, 3409);

        something.Received().SomeAction(Arg.Any<int>(), Arg.Any<Arg.AnyType>());
        something.Received().SomeAction(7, 3409);
    }

    [Test]
    public void When_Do_works_with_AnyType()
    {
        int? whenDoResult = null;
        bool whenDoCalled = false;
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .When(substitute => substitute.SomeAction(Arg.Any<int>(), Arg.Any<Arg.AnyType>()))
            .Do(info =>
            {
                whenDoResult = info.ArgAt<int>(1);
                whenDoCalled = true;
            });

        something.SomeAction(7, 3409);

        Assert.That(whenDoCalled, Is.True);
        Assert.That(whenDoResult, Is.EqualTo(3409));
    }

    [Test]
    public void ArgDo_works_with_AnyType()
    {
        string argDoResult = null;
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something.SomeAction(Arg.Any<int>(), Arg.Do<Arg.AnyType>(a => argDoResult = ">>" + ((int)a).ToString("P", CultureInfo.InvariantCulture)));

        something.SomeAction(7, 3409);

        Assert.That(argDoResult, Is.EqualTo(">>340,900.00 %"));
    }

    [Test]
    public void Is_matcher_works_with_AnyType()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeFunction(Arg.Any<int>(), Arg.Is<Arg.AnyType>(a => (int)a == 3409)).Returns("matched");

        var result = something.SomeFunction(7, 3409);

        Assert.That(result, Is.EqualTo("matched"));
    }

    [Test]
    public void Any_matcher_works_with_AnyType_and_constraints()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var state = new[] { 3409 };
        something.SomeActionWithGenericConstraints(7, state);

        something.Received().SomeActionWithGenericConstraints(Arg.Any<int>(), Arg.Any<MyAnyType>());
        something.Received().SomeActionWithGenericConstraints(7, state);
    }

    [Test]
    public void When_Do_works_with_AnyType_and_constraints()
    {
        int[] whenDoResult = null;
        bool whenDoCalled = false;
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .When(substitute => substitute.SomeActionWithGenericConstraints(Arg.Any<int>(), Arg.Any<MyAnyType>()))
            .Do(info =>
            {
                whenDoResult = info.ArgAt<int[]>(1);
                whenDoCalled = true;
            });

        var expected = new[] { 3409 };
        something.SomeActionWithGenericConstraints(7, expected);

        Assert.That(whenDoCalled, Is.True);
        Assert.That(whenDoResult, Is.EqualTo(expected));
    }

    [Test]
    public void ArgDo_works_with_AnyType_and_constraints()
    {
        string argDoResult = null;
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something.SomeActionWithGenericConstraints(Arg.Any<int>(), Arg.Do<MyAnyType>(a => argDoResult = ">>" + ((int[])a)[0].ToString("P", CultureInfo.InvariantCulture)));

        something.SomeActionWithGenericConstraints(7, new[] { 3409 });

        Assert.That(argDoResult, Is.EqualTo(">>340,900.00 %"));
    }

    [Test]
    public void Is_matcher_works_with_AnyType_and_constraints()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeFunctionWithGenericConstraints(Arg.Any<int>(), Arg.Is<MyAnyType>(a => ((int[])a)[0] == 3409)).Returns("matched");

        var result = something.SomeFunctionWithGenericConstraints(7, new[] { 3409 });

        Assert.That(result, Is.EqualTo("matched"));
    }

    [Test]
    public void Returns_works_with_AnyType_for_result_with_AnyType_generic_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .SomeFunction(Arg.Any<Arg.AnyType>())
            .Returns(x =>
            {
                return default!;
            });

        ICollection<int> result = something.SomeFunction(7);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Returns_works_with_AnyType_for_out_parameter_with_AnyType_generic_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .SomeFunctionWithOut(out Arg.Any<IEnumerable<Arg.AnyType>>())
            .Returns(true);

        bool result = something.SomeFunctionWithOut(out IEnumerable<int> value);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Returns_works_with_AnyType_for_ref_parameter_with_AnyType_generic_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .SomeFunctionWithRef(ref Arg.Any<IEnumerable<Arg.AnyType>>())
            .Returns(true);

        IEnumerable<int> refParameter = null;
        bool result = something.SomeFunctionWithRef(ref refParameter);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Callback_allows_access_to_method_call()
    {
        static ICollection<T> CreateSubstitute<T>(int count)
        {
            ICollection<T> substitute = Substitute.For<ICollection<T>>();
            substitute.Count.Returns(count);
            return substitute;
        }

        MethodInfo methodInfo = typeof(GenericArguments)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(x
                => x.Name.Contains(nameof(CreateSubstitute))
                && x.Name.Contains(nameof(Callback_allows_access_to_method_call)));

        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        something
            .SomeFunctionWithoutGenericMethodParameter<Arg.AnyType>(Arg.Any<int>())
            .Returns(x =>
            {
                Type argumentType = x.GenericArgs()[0];
                MethodInfo method = methodInfo.MakeGenericMethod(argumentType);
                return method.Invoke(null, [x.Arg<int>()]);
            });

        ICollection<int> result = something.SomeFunctionWithoutGenericMethodParameter<int>(7);

        Assert.That(result.Count, Is.EqualTo(7));
    }

    [Test]
    public void Matcher_for_wider_covariant_type_accepts_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<Cat>>());

        Assert.That(typeof(ICovariant<Animal>).IsAssignableFrom(typeof(ICovariant<Cat>)), Is.True);
        something.Received(1).SomeActionWithObject(Arg.Any<ICovariant<Animal>>());
    }

    [Test]
    public void Matcher_for_narrower_covariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<Animal>>());

        Assert.That(typeof(ICovariant<Cat>).IsAssignableFrom(typeof(ICovariant<Animal>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<ICovariant<Cat>>());
    }

    [Test]
    public void Matcher_for_sibling_covariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<Cat>>());

        Assert.That(typeof(ICovariant<Dog>).IsAssignableFrom(typeof(ICovariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<ICovariant<Dog>>());
    }

    [Test]
    public void Matcher_for_narrower_contravariant_type_accepts_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<IContravariant<Animal>>());

        Assert.That(typeof(IContravariant<Cat>).IsAssignableFrom(typeof(IContravariant<Animal>)), Is.True);
        something.Received(1).SomeActionWithObject(Arg.Any<IContravariant<Cat>>());
    }

    [Test]
    public void Matcher_for_wider_contravariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<IContravariant<Cat>>());

        Assert.That(typeof(IContravariant<Animal>).IsAssignableFrom(typeof(IContravariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<IContravariant<Animal>>());
    }

    [Test]
    public void Matcher_for_wider_invariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<IInvariant<Cat>>());

        Assert.That(typeof(IInvariant<Animal>).IsAssignableFrom(typeof(IInvariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<IInvariant<Animal>>());
    }

    [Test]
    public void Matcher_for_narrower_invariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<IInvariant<Animal>>());

        Assert.That(typeof(IInvariant<Cat>).IsAssignableFrom(typeof(IInvariant<Animal>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<IInvariant<Cat>>());
    }

    [Test]
    public void Matcher_does_not_apply_variance_to_value_type_arguments()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(new List<int> { 1 });

        Assert.That(typeof(IEnumerable<object>).IsAssignableFrom(typeof(List<int>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<IEnumerable<object>>());
    }

    [Test]
    public void Covariant_parameter_accepts_a_narrower_value()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithCovariantOfAnimal(Substitute.For<ICovariant<Cat>>());

        something.Received(1).SomeActionWithCovariantOfAnimal(Arg.Any<ICovariant<Animal>>());
        something.Received(1).SomeActionWithCovariantOfAnimal(Arg.Any<ICovariant<Cat>>());
    }

    [Test]
    public void Contravariant_parameter_accepts_a_wider_value()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithContravariantOfCat(Substitute.For<IContravariant<Animal>>());

        something.Received(1).SomeActionWithContravariantOfCat(Arg.Any<IContravariant<Cat>>());
    }

    [Test]
    public void Invariant_parameter_matches_its_own_type_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariantOfAnimal(Substitute.For<IInvariant<Animal>>());

        something.Received(1).SomeActionWithInvariantOfAnimal(Arg.Any<IInvariant<Animal>>());
    }

    [Test]
    public void Spec_with_wider_bare_type_argument_matches_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeAction<Cat>(7, new Cat());

        Assert.That(typeof(Animal).IsAssignableFrom(typeof(Cat)), Is.True);
        something.Received(1).SomeAction<Animal>(7, Arg.Any<Animal>());
    }

    [Test]
    public void Spec_with_narrower_bare_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var cat = new Cat();

        // Call the SomeAction<Animal> instantiation. Even though the passed value is a Cat,
        // this is *not* a call to the SomeAction<Cat> instantiation.
        something.SomeAction<Animal>(7, cat);

        Assert.That(typeof(Cat).IsAssignableFrom(typeof(Animal)), Is.False);
        something.Received().SomeAction<Animal>(7, Arg.Any<Animal>());
        something.DidNotReceive().SomeAction<Cat>(7, Arg.Any<Cat>());
    }

    [Test]
    public void Spec_with_sibling_bare_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeAction<Cat>(7, new Cat());

        something.DidNotReceive().SomeAction<Dog>(7, Arg.Any<Dog>());
    }

    [Test]
    public void Spec_with_wider_covariant_type_argument_matches_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithCovariant(Substitute.For<ICovariant<Cat>>());

        Assert.That(typeof(ICovariant<Animal>).IsAssignableFrom(typeof(ICovariant<Cat>)), Is.True);
        something.Received(1).SomeActionWithCovariant<Animal>(Arg.Any<ICovariant<Animal>>());
    }

    [Test]
    public void Spec_with_narrower_covariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithCovariant(Substitute.For<ICovariant<Animal>>());

        Assert.That(typeof(ICovariant<Cat>).IsAssignableFrom(typeof(ICovariant<Animal>)), Is.False);
        something.DidNotReceive().SomeActionWithCovariant<Cat>(Arg.Any<ICovariant<Cat>>());
    }

    [Test]
    public void Spec_with_sibling_covariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithCovariant(Substitute.For<ICovariant<Cat>>());

        something.DidNotReceive().SomeActionWithCovariant<Dog>(Arg.Any<ICovariant<Dog>>());
    }

    [Test]
    public void Spec_with_narrower_contravariant_type_argument_matches_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithContravariant(Substitute.For<IContravariant<Animal>>());

        Assert.That(typeof(IContravariant<Cat>).IsAssignableFrom(typeof(IContravariant<Animal>)), Is.True);
        something.Received(1).SomeActionWithContravariant<Cat>(Arg.Any<IContravariant<Cat>>());
    }

    [Test]
    public void Spec_with_wider_contravariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithContravariant(Substitute.For<IContravariant<Cat>>());

        Assert.That(typeof(IContravariant<Animal>).IsAssignableFrom(typeof(IContravariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithContravariant<Animal>(Arg.Any<IContravariant<Animal>>());
    }

    [Test]
    public void Spec_with_sibling_contravariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithContravariant(Substitute.For<IContravariant<Cat>>());

        something.DidNotReceive().SomeActionWithContravariant<Dog>(Arg.Any<IContravariant<Dog>>());
    }

    [Test]
    public void Spec_with_wider_invariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariant(Substitute.For<IInvariant<Cat>>());

        Assert.That(typeof(IInvariant<Animal>).IsAssignableFrom(typeof(IInvariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithInvariant<Animal>(Arg.Any<IInvariant<Animal>>());
    }

    [Test]
    public void Spec_with_narrower_invariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariant(Substitute.For<IInvariant<Animal>>());

        Assert.That(typeof(IInvariant<Cat>).IsAssignableFrom(typeof(IInvariant<Animal>)), Is.False);
        something.DidNotReceive().SomeActionWithInvariant<Cat>(Arg.Any<IInvariant<Cat>>());
    }

    [Test]
    public void Spec_with_same_invariant_type_argument_matches_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariant(Substitute.For<IInvariant<Cat>>());

        something.Received(1).SomeActionWithInvariant<Cat>(Arg.Any<IInvariant<Cat>>());
    }

    [Test]
    public void Configured_return_for_one_generic_instantiation_does_not_apply_to_another()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        // Configure the SomeFunctionWithOut<Cat> instantiation only.
        something.SomeFunctionWithOut<Cat>(out Arg.Any<IEnumerable<Cat>>()).Returns(true);

        // Calling a different instantiation must not pick up the configured return value.
        bool result = something.SomeFunctionWithOut<Animal>(out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Configured_return_for_a_wider_out_parameter_does_not_apply_to_a_narrower_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeFunctionWithOut<Animal>(out Arg.Any<IEnumerable<Animal>>()).Returns(true);

        bool result = something.SomeFunctionWithOut<Cat>(out _);

        // Type.IsAssignableFrom reports variance for by-ref types, unlike the language, so it
        // cannot be used as the oracle here.
        Assert.That(typeof(IEnumerable<Animal>), Is.Not.EqualTo(typeof(IEnumerable<Cat>)));
        Assert.That(result, Is.False);
    }

    [Test]
    public void Spec_with_narrower_unused_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithUnusedGenericArgument<Animal>();

        something.Received(1).SomeActionWithUnusedGenericArgument<Animal>();
        something.DidNotReceive().SomeActionWithUnusedGenericArgument<Cat>();
    }

    [Test]
    public void Spec_with_wider_unused_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithUnusedGenericArgument<Cat>();

        something.DidNotReceive().SomeActionWithUnusedGenericArgument<Animal>();
    }

    [Test]
    public void Configured_return_does_not_apply_across_bare_return_instantiations()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var cat = new Cat();

        something.SomeFunctionReturningGenericArgument<Animal>().Returns(cat);

        Assert.That(something.SomeFunctionReturningGenericArgument<Animal>(), Is.SameAs(cat));
        Assert.That(something.SomeFunctionReturningGenericArgument<Cat>(), Is.Not.SameAs(cat));
    }

    [Test]
    public void Configured_return_does_not_apply_across_covariant_return_instantiations()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var animals = Substitute.For<ICovariant<Animal>>();

        something.SomeFunctionReturningCovariant<Animal>().Returns(animals);

        Assert.That(something.SomeFunctionReturningCovariant<Animal>(), Is.SameAs(animals));
        Assert.That(something.SomeFunctionReturningCovariant<Cat>(), Is.Not.SameAs(animals));
    }

    [Test]
    public void Configured_return_does_not_apply_across_contravariant_return_instantiations()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var cats = Substitute.For<IContravariant<Cat>>();

        something.SomeFunctionReturningContravariant<Cat>().Returns(cats);

        Assert.That(something.SomeFunctionReturningContravariant<Cat>(), Is.SameAs(cats));
        Assert.That(something.SomeFunctionReturningContravariant<Animal>(), Is.Not.SameAs(cats));
    }

    [Test]
    public void Configured_return_value_may_be_narrower_than_the_declared_return_type()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        var cats = Substitute.For<ICovariant<Cat>>();

        something.SomeFunctionReturningCovariant<Animal>().Returns(cats);

        Assert.That(something.SomeFunctionReturningCovariant<Animal>(), Is.SameAs(cats));
    }

    [Test]
    public void Matcher_for_sibling_invariant_type_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<IInvariant<Cat>>());

        Assert.That(typeof(IInvariant<Dog>).IsAssignableFrom(typeof(IInvariant<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<IInvariant<Dog>>());
    }

    [Test]
    public void Spec_with_sibling_invariant_type_argument_does_not_match_the_call()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariant(Substitute.For<IInvariant<Cat>>());

        something.DidNotReceive().SomeActionWithInvariant<Dog>(Arg.Any<IInvariant<Dog>>());
    }

    [Test]
    public void Matcher_for_unrelated_type_argument_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<string>>());

        Assert.That(typeof(ICovariant<Cat>).IsAssignableFrom(typeof(ICovariant<string>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<ICovariant<Cat>>());
    }

    [Test]
    public void Matcher_for_covariant_type_with_value_type_argument_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<int>>());

        Assert.That(typeof(ICovariant<object>).IsAssignableFrom(typeof(ICovariant<int>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<ICovariant<object>>());
    }

    [Test]
    public void Matcher_for_legally_composed_variance_accepts_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<IContravariant<Animal>>>());

        Assert.That(
            typeof(ICovariant<IContravariant<Cat>>).IsAssignableFrom(typeof(ICovariant<IContravariant<Animal>>)),
            Is.True);
        something.Received(1).SomeActionWithObject(Arg.Any<ICovariant<IContravariant<Cat>>>());
    }

    [Test]
    public void Matcher_for_illegally_composed_variance_rejects_the_argument()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithObject(Substitute.For<ICovariant<IContravariant<Cat>>>());

        Assert.That(
            typeof(ICovariant<IContravariant<Animal>>).IsAssignableFrom(typeof(ICovariant<IContravariant<Cat>>)),
            Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<ICovariant<IContravariant<Animal>>>());
    }

    [Test]
    public void Matcher_for_narrower_delegate_type_accepts_the_wider_handler()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        Action<Animal> animalHandler = _ => { };

        something.SomeActionWithObject(animalHandler);

        Assert.That(typeof(Action<Cat>).IsAssignableFrom(typeof(Action<Animal>)), Is.True);
        something.Received(1).SomeActionWithObject(Arg.Any<Action<Cat>>());
    }

    [Test]
    public void Matcher_for_wider_delegate_type_rejects_the_narrower_handler()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        Action<Cat> catHandler = _ => { };

        something.SomeActionWithObject(catHandler);

        Assert.That(typeof(Action<Animal>).IsAssignableFrom(typeof(Action<Cat>)), Is.False);
        something.DidNotReceive().SomeActionWithObject(Arg.Any<Action<Animal>>());
    }

    [Test]
    public void Matcher_for_wider_func_type_accepts_the_narrower_factory()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();
        Func<Cat> catFactory = () => new Cat();

        something.SomeActionWithObject(catFactory);

        Assert.That(typeof(Func<Animal>).IsAssignableFrom(typeof(Func<Cat>)), Is.True);
        something.Received(1).SomeActionWithObject(Arg.Any<Func<Animal>>());
    }

    [Test]
    public void AnyType_matches_every_instantiation()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeAction<Animal>(7, new Cat());
        something.SomeAction<Cat>(7, new Cat());

        something.Received(2).SomeAction(7, Arg.Any<Arg.AnyType>());
    }

    [Test]
    public void AnyType_nested_in_a_generic_type_matches_every_instantiation()
    {
        ISomethingWithGenerics something = Substitute.For<ISomethingWithGenerics>();

        something.SomeActionWithInvariant(Substitute.For<IInvariant<Cat>>());
        something.SomeActionWithInvariant(Substitute.For<IInvariant<Animal>>());

        something.Received(2).SomeActionWithInvariant(Arg.Any<IInvariant<Arg.AnyType>>());
    }
}
