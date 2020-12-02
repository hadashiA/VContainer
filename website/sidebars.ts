module.exports = {
  docs: {
    'About': [
        // 'about/what-is-vcontainer'
        { type: 'doc', id: 'about/what-is-vcontainer' }
    ],
    'Setup': [
        'setup/installation',
        'setup/getting-started'
    ],
    'Injection': [
        'resolving/constructor-injection',
        'resolving/method-injection',
        'resolving/property-field-injection',
        'resolving/auto-inject-gameobjects',
        'resolving/implicit-relationship-types',
        'resolving/use-container-directory',
    ],
    'Registering': [
        'registering/register-type',
        'registering/register-factory',
        'registering/register-monobehaviour',
        'registering/register-scriptable-object',
    ],
    'Controlling Scope and Lifetime': [
        'scoping/lifetime-overview',
        'scoping/loading-additional-child',
        'scoping/generate-child-with-code-first',
        'scoping/project-root-lifetimescope'
    ],
    'Unity Integration': [
        'integration/dispatching-unity-lifecycle-event',
        'integration/ecs'
    ],
    'Optimization': [
        'optimization/codegen',
        'optimization/async-container-build',
        'optimization/parallel-container-build'
    ],
      // 'Comparing to Zenject': {
      //   type: 'doc',
      //   id: 'zenject',
      // }
  },
};