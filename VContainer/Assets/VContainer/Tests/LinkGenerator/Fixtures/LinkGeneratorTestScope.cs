using UnityEngine;
using VContainer.Unity;

namespace VContainer.Tests.LinkGenerator
{
    public sealed class LinkGeneratorTestScope : LifetimeScope
    {
        public FooMono1 t1;
        [SerializeField] private FooMono2 t2;
        public FooScriptable1 t3;
        [SerializeField] private FooScriptable2 t4;
        [SerializeReference] public BaseFoo t5;
        [SerializeReference] private BaseFoo t6;
        public FooSerializable1 t8;
        [SerializeField] private FooSerializable2 t9;
        public AssetBundle t10;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(t1);
            builder.RegisterComponent(t2);
            builder.RegisterComponent(t3);
            builder.RegisterComponent(t4);

            builder.RegisterInstance(t5);
            builder.RegisterInstance(t6);
            builder.RegisterInstance(t8);
            builder.RegisterInstance(t9);

            builder.RegisterInstance(t10);
        }
    }
}