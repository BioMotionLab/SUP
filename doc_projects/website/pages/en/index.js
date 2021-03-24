/**
 * Copyright (c) 2017-present, Facebook, Inc.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

const React = require('react');

const CompLibrary = require('../../core/CompLibrary.js');

const MarkdownBlock = CompLibrary.MarkdownBlock; /* Used to read markdown */
const Container = CompLibrary.Container;
const GridBlock = CompLibrary.GridBlock;

class HomeSplash extends React.Component {
  render() {
    const {siteConfig, language = ''} = this.props;
    const {baseUrl, docsUrl} = siteConfig;
    const docsPart = `${docsUrl ? `${docsUrl}/` : ''}`;
    const langPart = `${language ? `${language}/` : ''}`;
    const docUrl = doc => `${baseUrl}${docsPart}${langPart}${doc}`;

    const SplashContainer = props => (
      <div className="homeContainer">
        <div className="homeSplashFade">
          <div className="wrapper homeWrapper">{props.children}</div>
        </div>
      </div>
    );

    const Logo = props => (
      <div className="projectLogo">
        <img src={props.img_src} alt="Project Logo" />
      </div>
    );

    const ProjectTitle = props => (
      <h2 className="projectTitle">
        {props.title}
        <small>{props.tagline}</small>
      </h2>
    );

    const PromoSection = props => (
      <div className="section promoSection">
        <div className="promoRow">
          <div className="pluginRowBlock">{props.children}</div>
        </div>
      </div>
    );

    const Button = props => (
      <div className="pluginWrapper buttonWrapper">
        <a className="button" href={props.href} target={props.target}>
          {props.children}
        </a>
      </div>
    );

    return (
      <SplashContainer>
        <Logo img_src={`${baseUrl}img/favicon.ico`} />
        <div className="inner">
          <ProjectTitle tagline={siteConfig.tagline} title={siteConfig.title} />
          <PromoSection>
            <Button href={docUrl('GettingStarted.html')}>Learn</Button>
            <Button href={docUrl('Installation.html')}>Install</Button>
          </PromoSection>
        </div>
      </SplashContainer>
    );
  }
}

class Index extends React.Component {
  render() {
    const {config: siteConfig, language = ''} = this.props;
    const {baseUrl} = siteConfig;

    const Block = props => (
      <Container
        padding={['bottom', 'top']}
        id={props.id}
        background={props.background}>
        <GridBlock
          align="left"
          contents={props.children}
          layout={props.layout}
        />
      </Container>
    );


      const GUI = () => (
          <Block background="dark">
              {[
                  {
                      content:
                          'Load a single file, a folder, or a customized list of animations, and play them quickly and easily in Unity using a simple graphical user interface. Easy playback and viewing controls combined with quick annotations means you can manage your animations without hassle.',
                      image: `${baseUrl}img/designinterface.png`,
                      imageAlign: 'right',
                      title: 'Play, examine, and review animations using our included viewer scene',
                  },
              ]}
          </Block>
      );
    
    const Scripting = () => (
      <Block background="light">
        {[
          {
            content:
              'Easily add bmlSUP to existing projects. Load and play animations with only a few lines of code using our simple API, and bmlSUP will take care of the rest.',
            image: `${baseUrl}img/scriptexample.png`,
            imageAlign: 'left',
            title: 'Extend existing projects',
          },
        ]}
      </Block>
    );

    const Rendering = () => (
      <Block background="dark">
        {[
          {
            content:
              'Animated body models can be rendered in a variety of ways, including bones, joints, transparent and opaque meshes. Meshes support any Unity material, meaning realistic or cartoon textures can be applied. Rendering is compatible with VR and AR applications out of the box.',
            image: `${baseUrl}img/rendering.png`,
            imageAlign: 'right',
            title: 'Diferent rendering modes allow for infinite possibilities',
          },
        ]}
      </Block>
    );

    const BMLTUX = () => (
      <Block background="light">
          {[
              {
                  content:
                      'bmlSUP is 100% compatible with bmlTUX, another tool created by BiomotionLab. bmlTUX is a tool to quickly and easily create experiments in Unity Game Engine. Combined, these tools will let you create behavioural experiments with realistic body models in the span of minutes. Learn more at https://biomotionlab.github.io/TUX/',
                  image: `${baseUrl}img/bmltux.png`,
                  imageAlign: 'right',
                  title: 'Quickly create experiments with bmlSUP and bmlTUX',
              },
          ]}
      </Block>
    );


    const Features = () => (
      <Block layout="fourColumn">
        {[
          {
            content: 'Load animated SMPL body models from the AMASS Database right into Unity Game Engine',
            image: `${baseUrl}img/undraw_sculpting_1c9p.svg`,
            imageAlign: 'top',
            title: 'Realistic animated body models in Unity',
          },
          {
            content: 'Animate simple stick figures or realistic textured humans in virtual reality',
            image: `${baseUrl}img/undraw_shopping_eii3.svg`,
            imageAlign: 'top',
            title: 'Render the way you want',
          },
          {
            content: 'Not a pro programmer? Use our included player, or a simple API in your own projects',
            image: `${baseUrl}img/undraw_dev_focus.svg`,
            imageAlign: 'top',
            title: 'Minimal Coding',
          },
        ]}
      </Block>
    );

    const Showcase = () => {
      if ((siteConfig.users || []).length === 0) {
        return null;
      }

      const showcase = siteConfig.users
        .filter(user => user.pinned)
        .map(user => (
          <a href={user.infoLink} key={user.infoLink}>
            <img src={user.image} alt={user.caption} title={user.caption} />
          </a>
        ));

      const pageUrl = page => baseUrl + (language ? `${language}/` : '') + page;

      return (
        <div className="productShowcaseSection paddingBottom">
          <h2>Who is Using This?</h2>
          <p>This project is used by all these people</p>
          <div className="logos">{showcase}</div>
          <div className="more-users">
            <a className="button" href={pageUrl('users.html')}>
              More {siteConfig.title} Users
            </a>
          </div>
        </div>
      );
    };

    return (
      <div>
        <HomeSplash siteConfig={siteConfig} language={language} />
        <div className="mainContainer">
          <Features />
          <GUI />
          <Scripting />
          <Rendering />
          <BMLTUX />
        </div>
      </div>
    );
  }
}

module.exports = Index;
