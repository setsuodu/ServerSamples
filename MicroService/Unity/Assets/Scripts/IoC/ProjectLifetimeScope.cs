using VContainer;
using VContainer.Unity;

// 添加到 Root GameObject 相当于容器
public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 真正的「全局单例」
        //builder.Register<SaveSystem>(Lifetime.Singleton);
        //builder.Register<AudioManager>(Lifetime.Singleton);
        //builder.Register<GameConfig>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<GameManager>(Lifetime.Singleton);

        // 每场景单例
        //builder.Register<LevelManager>(Lifetime.Scoped);

        // 每次新实例（适合 UI Panel）
        //builder.Register<IU_GameView>(Lifetime.Transient);

        // 组件自动注入（加在场景中的 MonoBehaviour）
        builder.RegisterComponentInHierarchy<PlayerController>();
    }
}