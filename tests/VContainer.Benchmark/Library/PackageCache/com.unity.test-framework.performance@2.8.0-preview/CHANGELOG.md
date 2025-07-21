# Changelog

## [2.8.0] - 2021-03-16

Add overloads to measurements for overriding sample unit
Fix cases where cleanup throws an exception

## [2.7.0] - 2021-02-19

Reduce metadata overhead when running locally by caching dependencies
Remove the need for link.xml
Restructured documentation
Fixed method measurement IterationsPerMeasurement

## [2.6.0] - 2021-01-12

Add build configuration support

## [2.5.1] - 2021-01-05

Fix serialization for Performance Test Report window


## [2.5.0] - 2020-12-29

Add domain reload support
Switch from Newtonsoft.Json to Unity json module


## [2.4.1] - 2020-11-05

Metadata collection was made public

## [2.4.0] - 2020-09-16

Upgrade json dependency to release version
Reduced overhead introduced when running tests
Performance Test Report window updates:
Added CSV export option.
Added monitoring of results file timestemp to support auto refresh when a new file is found.
Added display of timestamp of last loaded results file.
Added option to sort test report window by order the tests ran in (index). This is now the default.
Added min and max to the table. 
Improve titles and tooltips on columns


## [2.3.1] - 2020-07-01

Fix overhead introduced with Measure.Method no longer calculates execution time of Setup and Cleanup changes

## [2.3.0] - 2020-06-17

Fix Measure.Method overhead
Measure.Method no longer caclulates execution time of Setup and Cleanup
Overwritten test name will be displyed with method name in Test Result viewer

## [2.2.0] - 2020-05-26

Add support for custom metadata

## [2.1.0] - 2020-05-14

Add flexible horizontal splitter for report window
Fix date format

## [2.0.9] - 2020-03-23

Fix profiler measurements for method measurements
Throw exceptions when measuring NaN

## [2.0.8] - 2020-02-20

Fix profiler marker capture when changing scenes in editor tests
Only shift samplegroups for UI

## [2.0.7] - 2020-02-14

Fix results parsing

## [2.0.6] - 2020-01-13

Fix development player field

## [2.0.5] - 2020-01-13

Disallow multiple performance attributes
Disallow empty samplegroup name
Assign samplegroup name to frames measurements

## [2.0.4] - 2019-12-05

Update json packae to support AOT platforms

## [2.0.3] - 2019-11-20

Add new fields to data format BuildTarget, StereoRenderingPath

## [2.0.2] - 2019-11-20

Increase test serialization version

## [2.0.1] - 2019-11-20

Fix player callbacks when no tests were executed

## [2.0.0] - 2019-11-19

Refactor data format, reduced nesting
Slight refactor on measurement API
Removed unused fields
Remove deprecated attributes
Shift sample units when printing results
Switch to newtosoft json package
Fix resources cleanup meta files
Add tests to package testables


## [1.3.1] - 2019-11-05

Fix warning after cleaning resources
Fix test suite when running in the editor

## [1.3.0] - 2019-08-26

Remove metadata collectors tests
Switch to errors from exceptions when parsing results
Increase minimum unity version to 2019.3

## [1.2.6] - 2019-08-22

### Categorize performance tests as `performance`

Categorize performance tests as performance
Remove profiler section on docs as the feature was removed
ProfilerMarkers can now be called with string params
Switch measuring frames and methods to stopwatch

## [1.2.5] - 2019-06-17

### Test publish for CI

## [1.2.4] - 2019-06-17

### Test publish for CI

## [1.2.3] - 2019-06-14

### Update changelog

## [1.2.2] - 2019-06-13

### Add support for domain reload

## [1.2.1] - 2019-06-07

### Fix bug that would cause player build failures

## [1.2.0] - 2019-05-23

### Increase unity version to 2019.2

## [1.1.0] - 2019-05-22

### Update assembly definition formats to avoid testables in package manifest

## [1.0.9] - 2019-05-21

#### Update scripting runtime setting for 2019.3

## [1.0.8] - 2019-03-08

#### Automation test deploy

## [1.0.7] - 2019-03-08

#### Automation test deploy

## [1.0.6] - 2019-03-04

### Update changelog

## [1.0.5] - 2019-03-04

### Add conditional support for 2019.1

## [1.0.4] - 2019-02-18

### remove unnecessary meta files

## [1.0.3] - 2019-02-18

### package.json update

## [1.0.2] - 2019-02-18

### package.json update

## [1.0.1] - 2019-02-18

### Updated Documentation to reflect breaking changes

## [1.0.0] - 2019-02-15

### Refactor attributes

## [0.1.50] - 2019-01-15

### Change results paths to persistent data

## [0.1.49] - 2018-12-04

### Revert changes to profiler and GC

## [0.1.48] - 2018-11-22

### Doc updates and ignore GC api in editor due to api issues

## [0.1.47] - 2018-11-14

### Remove debug logs

## [0.1.46] - 2018-11-14

### Fix breaking changes introduced by testrunner API rename

## [0.1.45] - 2018-11-08

### Fix breaking changes to data submodule

## [0.1.44] - 2018-11-08

### Disable GC and update API to work around warning

## [0.1.43] - 2018-10-30

### Fix method measurements setup and cleanup

## [0.1.42] - 2018-10-15

### Improvements to report window and minor fixes

Save profiler output on perf tests
Add a button on report window to open profiler output for test
Remove unsupported features for legacy scripting runtime
Fix version attribute for test cases
Remove unnecessary assembly definition

## [0.1.41] - 2018-10-02

### Test report graph

## [0.1.40] - 2018-09-17

### Update documentation

## [0.1.39] - 2018-09-14

### Remove duplicate module from docs

## [0.1.38] - 2018-09-14

### Documentation updates

## [0.1.36] - 2018-08-27

### ProfilerMarkers now take params as arguments

## [0.1.35] - 2018-08-27

### Measure.Method improvements

Add GC allocation to Measure.Method
Add setup/cleanup for Measure.Method
Move order of calls for Measure.Scope

## [0.1.34] - 2018-08-16

### Obsolete warnings

## [0.1.33] - 2018-08-03

### Small fixes

Obsolete warnings, doc update with modules and internals, ValueSource fix

## [0.1.32] - 2018-07-09

### Add custom measurement/warmup counts

Method and Frames measurements can now specify custom warmup, measurement and iteration counts

## [0.1.31] - 2018-07-04

### mark metadata tests with performance category

## [0.1.30] - 2018-06-27

### fix Method measurement

## [0.1.29] - 2018-06-12

### Moving back to json in xml due to multiple instabilities


## [0.1.28] - 2018-06-01

### Remove json printing from output


## [0.1.27] - 2018-05-31

### Add meta files to npm ignore


## [0.1.26] - 2018-05-31

### Preparing package for moving to public registry

Inversed changelog order
Excluded CI files from published package


## [0.1.25] - 2018-05-31

### Remove missing meta files


## [0.1.24] - 2018-05-31

### Print out json to xml by default for backwards compatability


## [0.1.23] - 2018-05-30

### Issues with packman, bumping up version

Issues with packman, bumping up version


## [0.1.22] - 2018-05-29

### Measure.Method Execution and Warmup count

Can now specify custom execution and warmup count


## [0.1.21] - 2018-05-25

### Fix issues introduced by .18 fix


## [0.1.19] - 2018-05-24

### Rename package

Package has been renamed to `com.unity.test-framework.performance` to match test framework


## [0.1.18] - 2018-05-24

### Fix SetUp and TearDown for 2018.1


## [0.1.17] - 2018-05-23

### Meatada collecting and changes to method/frames measurements

Refactor Method and Frames measurements
Metadata collected using internal test runner API and player connection for 2018.3+


## [0.1.16] - 2018-05-09

### Bug fix

Bug fix regarding measureme methods being disposed twice


## [0.1.15] - 2018-05-02

### Bug fix for metadata test

The test was failing if a json file was missing for playmode tests


## [0.1.14] - 2018-04-30

### Measure method refactor

Introduced SampleGroupDefinition
Addition of measuring a method or frames for certain amount of times or for duration
Refactored measuring methods
Removes linq usage for due to issues with AOT platforms


## [0.1.13] - 2018-04-15

### Updates to aggregation and metadata for android

Fixed android metadata collecting
Removed totaltime from frametime measurements
Added total, std and sample count aggregations
Added sample unit to multi sample groups


## [0.1.12] - 2018-04-11

### Change naming and fix json serialization


## [0.1.11] - 2018-04-09

### Fix 2018.1 internal namespaces

Fix 2018.1 internal namespaces


## [0.1.10] - 2018-04-09

### Collect metadata and update coding style

Change fields to UpperCamelCase
Added editmode and playmode tests that collect metadata


## [0.1.9] - 2018-04-06

### Add json output for 2018.1

After test run, we will now print json output


## [0.1.8] - 2018-04-03

### Fix for 2018.1

Fix an exception on 2018.1


## [0.1.7] - 2018-04-03

### improvements to overloads and documentation

Changed some of the names to match new convention
Addressed typos in docs
Multiple overloads replaced by using default arguments


## [0.1.6] - 2018-03-28

### improvements to overloads and documentation

Measure.Custom got a new overload with SampleGroup
Readme now includes installation and more examples


## [0.1.5] - 2018-03-20

### Adding checks for usage outside of Performance tests

Adding checks for usage outside of Performance tests


## [0.1.4] - 2018-03-20

### Adding system info to performance test output

Preparing for reporting test data


## [0.1.3] - 2018-03-14

### Removed tests

Temporarily removing tests from the package into separate repo.


## [0.1.2] - 2018-03-14

### Bug fix

Update for a missing bracket


## [0.1.1] - 2018-03-14

### Updates to test results and measurement methods

Test output now includes json that can be used to parse performance data from TestResults.xml
Added defines to be compatible with 2018.1 and newer
Removed unnecessary overloads for measurements due to introduction of SampleGroup
Measurement methods can now take in SampleGroup as argument.


## [0.1.0] - 2018-02-27

### This is the first release of *Unity Package performancetesting*.

Initial version.
