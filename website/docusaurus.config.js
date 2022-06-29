module.exports = {
  title: 'VContainer',
  tagline: 'The extra fast DI (Dependency Injection) for Unity Game Engine',
  url: 'https://vcontainer.hadashikick.jp',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.png',
  organizationName: 'hadashiA', // Usually your GitHub org/user name.
  projectName: 'VContainer', // Usually your repo name.
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ja']
  },
  themeConfig: {
    metadata: [
      { name: 'google-site-verification', content: 'ldYnOkZTq5AfzmJzEbsFzWXAYp9tyO5IhmYQv45MMDY' },
    ],
    image: 'img/vcontainer@2x.png',
    colorMode: {
      // Hides the switch in the navbar
      // Useful if you want to support a single color mode
      disableSwitch: false,

      // Should we use the prefers-color-scheme media-query,
      // using user system preferences, instead of the hardcoded defaultMode
      respectPrefersColorScheme: false,
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
          type: 'localeDropdown',
          position: 'left',
        },
        {
          to: 'getting-started/installation',
          activeBasePath: 'none',
          label: 'Getting Started',
          position: 'right',
        },
        {
          href: 'https://github.com/hadashiA/VContainer/releases',
          label: 'v1.11.1',
          position: 'right',
        },
        {
          href: 'https://github.com/hadashiA/VContainer',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      copyright: `Copyright © ${new Date().getFullYear()} <a href="https://twitter.com/hadashiA">hadashiA</a>`,
      // logo: {
      //   alt: 'VContainer',
      //   src: 'img/favicon.png',
      //   href: 'https://github.com/hadashiA/VContainer',
      // },
      // links: [
      //   {
      //     title: 'Author',
      //     items: [
      //       {
      //         label: '@hadashiA',
      //         href: 'https://twitter.com/hadashiA',
      //       },
      //     ],
      //   }
      // ]
    },
    algolia: {
      appId: process.env.ALGOLIA_APP_ID,
      apiKey: process.env.ALGOLIA_API_KEY,
      indexName: process.env.ALGOLIA_INDEX_NAME
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.ts'),
          sidebarCollapsed: false,
          sidebarCollapsible: false,
          routeBasePath: '/',
          // Please change this to your repo.
          editUrl: 'https://github.com/hadashiA/VContainer/edit/master/website/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
        googleAnalytics: {
          trackingID: process.env.GA_TRACKING_ID,
          // Optional fields.
          anonymizeIP: true, // Should IPs be anonymized?
        },
      },
    ],
  ],
}
