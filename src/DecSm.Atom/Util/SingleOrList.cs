namespace DecSm.Atom.Util;

[PublicAPI]
[Union]
public partial record SingleOrList<T>
{
    public sealed partial record Single(T Value);

    public sealed partial record List(List<T> Value);
}
