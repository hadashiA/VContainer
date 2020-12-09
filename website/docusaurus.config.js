module.exports = {
  title: 'VContainer',
  tagline: 'A fastest DI (Dependency Injection) for Unity (Game Engine)',
  url: 'https://vcontainer.hadashikick.jp',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.png',
  organizationName: 'hadashiA', // Usually your GitHub org/user name.
  projectName: 'VContainer', // Usually your repo name.
  themeConfig: {
    colorMode: {
      // Hides the switch in the navbar
      // Useful if you want to support a single color mode
      disableSwitch: false,

      // Should we use the prefers-color-scheme media-query,
      // using user system preferences, instead of the hardcoded defaultMode
      respectPrefersColorScheme: false,

      // Dark/light switch icon options
      switchConfig: {
        // Icon for the switch while in dark mode
        darkIcon: '🌙',

        // CSS to apply to dark icon,
        // React inline style object
        // see https://reactjs.org/docs/dom-elements.html#style
        darkIconStyle: {
          marginLeft: '2px',
        },

        // Unicode icons such as '\u2600' will work
        // Unicode with 5 chars require brackets: '\u{1F602}'
        lightIcon: '\u{1F602}',

        lightIconStyle: {
          marginLeft: '1px',
        },
      },
    },
    prism: {
      additionalLanguages: ['csharp'],
      theme: require('prism-react-renderer/themes/vsDark'),
      darkTheme: require('prism-react-renderer/themes/vsDark'),
    },
    navbar: {
      title: 'VContainer',
      // logo: {
      //   alt: 'My Site Logo',
      //   src: 'img/logo.svg',
      // },
      items: [
        {
          to: 'about/getting-started',
          activeBasePath: 'none',
          label: 'Getting Started',
          position: 'right',
        },
        {
          href: 'https://github.com/hadashiA/VContainer/releases',
          label: 'v1.3.0',
          position: 'right',
        },
        {
          href: 'https://github.com/hadashiA/VContainer',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    // footer: {
    //   style: 'dark',
    //   links: [
    //     {
    //       title: 'Docs',
    //       items: [
    //         {
    //           label: 'Getting Started',
    //           to: 'docs/getting-started',
    //         },
    //       ],
    //     },
    //     {
    //       title: 'Community',
    //       items: [
    //         {
    //           label: 'Twitter',
    //           href: 'https://twitter.com/hadashiA',
    //         },
    //       ],
    //     },
    //     {
    //       title: 'More',
    //       items: [
    //         {
    //           label: 'GitHub',
    //           href: 'https://github.com/hadashiA/VContainer',
    //         },
    //       ],
    //     },
    //   ],
    // },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.ts'),
          routeBasePath: '/',
          // Please change this to your repo.
          editUrl: 'https://github.com/hadashiA/VContainer/edit/master/website/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
}
