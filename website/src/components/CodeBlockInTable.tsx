import * as React from "react"
import { Highlight, themes } from 'prism-react-renderer';

const indent = (code: string) => code.split(/[\s]+/).join("\n    ")

export const Inline: React.FC<React.PropsWithChildren<{whitespaces: number}>> = ({children}) => {
  if (React.Children.count(children) != 1) {
    return children
  }

  const code = React.Children.toArray(children)[0]
  if (typeof code != 'string') {
    return children
  }

  return <Highlight code={indent(code)} language="csharp" theme={themes.vsDark}>
    {({ className, style, tokens, getLineProps, getTokenProps }) => (
      <pre className={className} style={style}>
        {tokens.map((line, i) => (
          <div {...getLineProps({ line, key: i })}>
            {line.map((token, key) => (
              <span {...getTokenProps({ token, key })} />
            ))}
          </div>
        ))}
      </pre>
    )}
  </Highlight>
}
