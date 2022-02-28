# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [5.0.0](https://github.com/GetStream/stream-net/compare/4.8.0...5.0.0) (2022-02-28)


### Features

- In v5.0.0, [we have refactored the library](https://github.com/GetStream/stream-net/pull/67) to be more maintainable in the future.
Most importantly, we got rid of some complex internal logic (such as tricky json serialization and deserialization, code organization improvements etc.).
Also, we made the library more modern such as adding `Async` postfix to async methods. All public
methods have documentation now and a link to the official docs now. This README file's code snippets are updated to reflect the new changes.

### 🚨 Breaking changes:
- All async methods have `Async` postfix.
- Model classes have been moved into `Stream.Models` namespace.
- All client classes have interfaces now, and `Ref()` methods are not static anymore. This will make it easier to the consumers of this library to unit test them.

## [4.8.0](https://github.com/GetStream/stream-net/compare/4.7.0...4.8.0) (2022-02-23)


### Features

* add overload for setdata ([#68](https://github.com/GetStream/stream-net/issues/68)) ([4e6a867](https://github.com/GetStream/stream-net/commit/4e6a867f13f4ed1d80a477de2b52063008a61ca9))

## [4.7.0](https://github.com/GetStream/stream-net/compare/4.6.0...4.7.0) (2022-02-04)


### Features

* add custom serializer support and .net6 support ([#63](https://github.com/GetStream/stream-net/issues/63)) ([5c30e5e](https://github.com/GetStream/stream-net/commit/5c30e5e03348ac52d3e82ac18b1daa73482cc172))

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
