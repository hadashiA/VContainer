import React from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {Redirect} from "@docusaurus/router"

const Home = () => {
  const context = useDocusaurusContext();
  const {siteConfig = {}} = context;
  return (
      <Redirect to="docs/" />
  );
}

export default Home;
