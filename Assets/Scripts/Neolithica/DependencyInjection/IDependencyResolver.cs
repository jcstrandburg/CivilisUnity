using System;

namespace Neolithica.DependencyInjection {
    public interface IDependencyResolver {
        object Resolve();
        Type DependencyType { get; }
    }
}