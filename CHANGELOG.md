# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [6.6.0-prerelease](https://github.com/GetStream/stream-net/compare/6.5.3...6.6.0-prerelease) (2023-07-31)

### [6.5.3](https://github.com/GetStream/stream-net/compare/6.5.2...6.5.3) (2023-05-16)

### [6.5.2](https://github.com/GetStream/stream-net/compare/6.5.1...6.5.2) (2023-01-31)

### [6.5.1](https://github.com/GetStream/stream-net/compare/6.5.0...6.5.1) (2022-10-04)

## [6.5.0](https://github.com/GetStream/stream-net/compare/6.4.0...6.5.0) (2022-06-28)


### Features

* add custom request parameters ([#93](https://github.com/GetStream/stream-net/issues/93)) ([115f324](https://github.com/GetStream/stream-net/commit/115f32419ff75e26c72704a919e95f9d413f418f))

## [6.4.0](https://github.com/GetStream/stream-net/compare/6.3.0...6.4.0) (2022-06-24)


### Features

* add id based to target update ([#90](https://github.com/GetStream/stream-net/issues/90)) ([6c1922c](https://github.com/GetStream/stream-net/commit/6c1922c1928c1f0602e8e84e9d2e1348e366e989))
* add new reaction query params ([#88](https://github.com/GetStream/stream-net/issues/88)) ([b2fad8c](https://github.com/GetStream/stream-net/commit/b2fad8c8e27e276dfa871f480a2431f8f557a641))
* add ref helpers for act/reaction ([#91](https://github.com/GetStream/stream-net/issues/91)) ([dad0785](https://github.com/GetStream/stream-net/commit/dad0785e1cbda8ca96dda5a41bd4f3116d370c9f))


### Bug Fixes

* filter query param for enrichment ([#89](https://github.com/GetStream/stream-net/issues/89)) ([e0d9c52](https://github.com/GetStream/stream-net/commit/e0d9c52f976d478a5cb515812fe08fda3a4e489b))

## [6.3.0](https://github.com/GetStream/stream-net/compare/6.2.0...6.3.0) (2022-06-22)


### Features

* add enriched personalized feed read support ([#86](https://github.com/GetStream/stream-net/issues/86)) ([8258f2c](https://github.com/GetStream/stream-net/commit/8258f2c1900097c4b591902af4384f72bc442416))

## [6.2.0](https://github.com/GetStream/stream-net/compare/6.1.2...6.2.0) (2022-06-02)


### Features

* add feedid validator ([#81](https://github.com/GetStream/stream-net/issues/81)) ([d7863ef](https://github.com/GetStream/stream-net/commit/d7863ef90502b1bc07836b4b32ebfdb9541a985c))
* **reaction:** add id overrides for child ([#83](https://github.com/GetStream/stream-net/issues/83)) ([b8a26aa](https://github.com/GetStream/stream-net/commit/b8a26aa229a902874c0897ba2b7d3689dfb7c7ad))


### Bug Fixes

* ensure error response properly serialized ([#80](https://github.com/GetStream/stream-net/issues/80)) ([76c52ce](https://github.com/GetStream/stream-net/commit/76c52cec89da7efd690c80821a69f802bec7ce5f))

### [6.1.2](https://github.com/GetStream/stream-net/compare/6.1.1...6.1.2) (2022-05-24)


### Bug Fixes

* **generator:** adjust documentation ([#78](https://github.com/GetStream/stream-net/issues/78)) ([effd27f](https://github.com/GetStream/stream-net/commit/effd27f075a62a37653675d8948e328d812a5ad4))

### [6.1.1](https://github.com/GetStream/stream-net/compare/6.1.0...6.1.1) (2022-05-24)


### Features

* **id_generator:** swap params ([#76](https://github.com/GetStream/stream-net/issues/76)) ([5d68fb8](https://github.com/GetStream/stream-net/commit/5d68fb89e9294690d0610c1a32feda3987e509a5))

## [6.1.0](https://github.com/GetStream/stream-net/compare/6.0.0...6.1.0) (2022-05-24)


### Features

* **reaction:** add overload for reactionId ([#74](https://github.com/GetStream/stream-net/issues/74)) ([0696f98](https://github.com/GetStream/stream-net/commit/0696f98423f0f783a03860e48dfcf831eab0f17f))

## [6.0.0](https://github.com/GetStream/stream-net/compare/5.0.0...6.0.0) (2022-03-18)


### Bug Fixes

* send flattened data when upserting ([#72](https://github.com/GetStream/stream-net/issues/72)) ([a3b1fbc](https://github.com/GetStream/stream-net/commit/a3b1fbcac8527da23e3ceea53b69f05513e25266))

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
