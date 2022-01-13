# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [4.6.0](https://github.com/GetStream/stream-net/compare/4.5.0...4.6.0) (2022-01-13)


### Features

* add possibility to query enriched activities by ids ([#58](https://github.com/GetStream/stream-net/issues/58)) ([612fa63](https://github.com/GetStream/stream-net/commit/612fa6377885368f1bd98322ed53bcb0736e8313))
* small enhancement ([#60](https://github.com/GetStream/stream-net/issues/60)) ([7da61ee](https://github.com/GetStream/stream-net/commit/7da61ee2819f7007d18443489d99e61d5658c9a0))

## 4.5.0 - 2021-11-19

* Add file/image delete support

## 4.4.1 - 2021-11-01

* Fix nuget release key encryption

## 4.4.0 - 2021-11-01

* Make some id of activity and id, kind, user_id and data of reaction publicly settable
  - so that more object creation flows are supported easily

## 4.3.0 - 2021-03-10
* Add follow stats endpoint support
* Add .Net 5 support

## 4.2.0 - 2021-02-26
* Add open graph scraping support

## 4.1.1 - 2021-02-26
* Naming consistency

## 4.1.0 - 2021-02-26
* Add upload support for files and images
* Add .net core 2.1 and 3.1 support
* Improve repo a bit in terms of formatting and static code analysis

## 4.0.0. - 2021-02-05
* Add support for client side token
* Replace `CreatUserSessionToken` with `CreateUserToken`

## 3.1.0 - 2021-02-01
* Improve underlying HTTP client usage
  - fixes socket exhaustaion
  - improves performance
* Move hardcoded keys from Github

## 3.0.2 - 2021-01-25
* Make version header consistent between SDKs

## 3.0.1 - 2021-01-15
* Add version and also include in request headers

## 3.0.0 - 2021-01-08
* Drop request signing and use only JWT
* Drop obsolete regions
* Fix post/delete of personalization tokens
* Start a changelog

## 2.13.0 - 2021-01-07
* Add personalization
* Bump copyright notice year
