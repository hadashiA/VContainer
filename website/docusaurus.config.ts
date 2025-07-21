import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';
import lunrSearchPlugin from "docusaurus-lunr-search";

const config: Config = {
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
    locales: ['en', 'ja'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          sidebarCollapsed: false,
          sidebarCollapsible: false,
          routeBasePath: '/',
          editUrl: 'https://github.com/hadashiA/VContainer/edit/master/website/',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],
  scripts: [
    {
      src: '/loadbutton.js',
      defer: true,
      async: true
    }
  ],
  plugins: [
    [lunrSearchPlugin, {
      languages: ["en", "ja"],
    }]
  ],
  themeConfig: {
    // Replace with your project's social card
    image: 'img/vcontainer@2x.png',
    metadata: [
      { name: 'google-site-verification', content: 'ldYnOkZTq5AfzmJzEbsFzWXAYp9tyO5IhmYQv45MMDY' },
    ],
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
          'label': 'v1.15.0',
          position: 'right',
        },
        {
          href: 'https://github.com/hadashiA/VContainer',
          // label: 'Github',
          position: 'right',
          className: 'github-button'
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
    },
    prism: {
      additionalLanguages: ['csharp'],
      theme: prismThemes.vsDark,
      darkTheme: prismThemes.vsDark,
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
