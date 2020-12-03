module.exports = {
  docs: {
      'ABOUT': [
          // 'about/what-is-vcontainer'
          { type: 'doc', id: 'about/concepts' },
          { type: 'doc', id: 'about/what-is-di' },
      ],
      'SETUP': [
          'setup/installation',
          'setup/getting-started'
      ],
      'RESOLVING': [
          'resolving/constructor-injection',
          'resolving/method-injection',
          'resolving/property-field-injection',
          'resolving/auto-inject-gameobjects',
          'resolving/implicit-relationship-types',
          'resolving/use-container-directory',
      ],
      'REGISTERING': [
          'registering/register-type',
          'registering/register-factory',
          'registering/register-monobehaviour',
          'registering/register-scriptable-object',
      ],
      'SCOPING': [
          'scoping/lifetime-overview',
          'scoping/loading-additional-child',
          'scoping/generate-child-with-code-first',
          'scoping/project-root-lifetimescope'
      ],
      'UNITY INTEGRATION': [
          'integration/dispatching-unity-lifecycle-event',
          'integration/ecs'
      ],
      'OPTIMIZATION': [
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