import * as React from "react"
import { ResponsiveBar } from '@nivo/bar'
import { useGraphTheme } from "./GraphThemeContext"

const data = [
  {
    "content": "DLL",
    "VContainer": 24,
    "Zenject": 478,
  },
  {
    "content": "IL2CPP C/C++",
    "VContainer": 1078,
    "Zenject": 9469,
  },
]

export function CodeSizeGraph({ height }: { height: number }): JSX.Element {
  const { theme } = useGraphTheme()

  return <div style={{height, width: '100%'}}>
    <ResponsiveBar
      theme={theme}
      data={data}
      groupMode="grouped"
      layout="vertical"
      keys={['VContainer', 'Zenject']}
      indexBy="content"
      margin={{top: 20, right: 60, bottom: 70, left: 60}}
      padding={0.3}
      valueScale={{type: 'linear'}}
      indexScale={{type: 'band', round: true}}
      colors={[
        'rgb(97, 205, 187)',
        'rgb(244, 117, 96)',
      ]}
      // defs={[
      //   {
      //     id: 'dots',
      //     type: 'patternDots',
      //     // background: 'inherit',
      //     background: 'rgb(97, 205, 187)',
      //     color: '#38bcb2',
      //     size: 4,
      //     padding: 1,
      //     stagger: true
      //   },
      //   {
      //     id: 'lines',
      //     type: 'patternLines',
      //     // background: 'inherit',
      //     background: 'rgb(241, 225, 91)',
      //     color: '#eed312',
      //     rotation: -45,
      //     lineWidth: 6,
      //     spacing: 10
      //   }
      // ]}
      // fill={[
      //   {
      //     match: {id: 'VContainer (CodeGen)'},
      //     id: 'lines'
      //   },
      // ]}
      borderColor={{from: 'color', modifiers: [['darker', 1.6]]}}
      axisTop={null}
      axisRight={null}
      axisBottom={{
        tickSize: 5,
        tickPadding: 0,
        tickRotation: 0,
        // renderTick: renderLabel,
        legend: '',
        legendPosition: 'middle',
        legendOffset: 32
      }}
      axisLeft={{
        tickSize: 6,
        tickPadding: 5,
        tickRotation: 0,
        tickValues: 3,
        legend: 'KB',
        legendPosition: 'middle',
        legendOffset: -50
      }}
      enableLabel={false}
      labelSkipWidth={12}
      labelSkipHeight={12}
      labelTextColor={{from: 'color', modifiers: [['darker', 1.6]]}}
      legends={[
        {
          dataFrom: 'keys',
          anchor: 'bottom-left',
          direction: 'row',
          justify: false,
          translateX: -15,
          translateY: 60,
          itemsSpacing: 2,
          itemWidth: 160,
          itemHeight: 20,
          itemDirection: 'left-to-right',
          itemOpacity: 0.85,
          symbolSize: 20,
          effects: [
            {
              on: 'hover',
              style: {
                itemOpacity: 1
              }
            }
          ]
        }
      ]}
      animate={true}
      motionStiffness={90}
      motionDamping={15}
    />
  </div>
}