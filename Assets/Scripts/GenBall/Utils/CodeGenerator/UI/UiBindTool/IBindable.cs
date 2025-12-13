namespace GenBall.Utils.CodeGenerator.UI
{
    public interface IBindable
    {
        public TypeEnum Type { get; }
    }

    public enum TypeEnum
    {
        Form,Item,Undefined
    }
}