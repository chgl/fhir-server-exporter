# Changelog

## [3.0.4](https://github.com/chgl/fhir-server-exporter/compare/v3.0.3...v3.0.4) (2026-02-10)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#457](https://github.com/chgl/fhir-server-exporter/issues/457)) ([4129d42](https://github.com/chgl/fhir-server-exporter/commit/4129d42101d43282c89e1dcc651b0cb14425f47b))
* **deps:** update all non-major dependencies ([#464](https://github.com/chgl/fhir-server-exporter/issues/464)) ([19d45a1](https://github.com/chgl/fhir-server-exporter/commit/19d45a15bb1b96a29a349f288333965152a43796))
* **deps:** update all non-major dependencies ([#466](https://github.com/chgl/fhir-server-exporter/issues/466)) ([0c05f46](https://github.com/chgl/fhir-server-exporter/commit/0c05f4697b6cea87b89cffd3febf871277c2f1f8))
* **deps:** update dependency duckdb.net.data.full to 1.4.4 ([#460](https://github.com/chgl/fhir-server-exporter/issues/460)) ([791c34c](https://github.com/chgl/fhir-server-exporter/commit/791c34c3448cfb86073da798e0a505a64f809d12))
* **deps:** update dependency meziantou.analyzer to 2.0.298 ([#463](https://github.com/chgl/fhir-server-exporter/issues/463)) ([d0ee178](https://github.com/chgl/fhir-server-exporter/commit/d0ee1789fb5a4e2215e6011cd00b5bf62f1cd459))
* **deps:** update dotnet monorepo ([#462](https://github.com/chgl/fhir-server-exporter/issues/462)) ([71ccc6e](https://github.com/chgl/fhir-server-exporter/commit/71ccc6ecb3960a7529523b3bbdbb216342e86bf7))
* **deps:** update github-actions ([#458](https://github.com/chgl/fhir-server-exporter/issues/458)) ([c188dc3](https://github.com/chgl/fhir-server-exporter/commit/c188dc3852f92c2bf486354beca0f20443c27ee2))
* **deps:** update github-actions ([#461](https://github.com/chgl/fhir-server-exporter/issues/461)) ([840436f](https://github.com/chgl/fhir-server-exporter/commit/840436f2699bb9935253208868885ca1f49f2de7))
* **deps:** update github-actions ([#465](https://github.com/chgl/fhir-server-exporter/issues/465)) ([b4d3826](https://github.com/chgl/fhir-server-exporter/commit/b4d38262fdd7c4c51f48a20632004240c57e0f8f))

## [3.0.3](https://github.com/chgl/fhir-server-exporter/compare/v3.0.2...v3.0.3) (2026-01-24)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#442](https://github.com/chgl/fhir-server-exporter/issues/442)) ([c928163](https://github.com/chgl/fhir-server-exporter/commit/c9281637714e52629659217bd728de474d39b353))
* **deps:** update all non-major dependencies ([#456](https://github.com/chgl/fhir-server-exporter/issues/456)) ([3341225](https://github.com/chgl/fhir-server-exporter/commit/3341225824716698de86248b7fae7629433ff565))
* **deps:** update github-actions ([#454](https://github.com/chgl/fhir-server-exporter/issues/454)) ([69234f5](https://github.com/chgl/fhir-server-exporter/commit/69234f53ca8cfe2a618b8931d5201b5550768c00))

## [3.0.2](https://github.com/chgl/fhir-server-exporter/compare/v3.0.1...v3.0.2) (2026-01-17)


### Bug Fixes

* duckdb extension path in container image ([#452](https://github.com/chgl/fhir-server-exporter/issues/452)) ([b1afb53](https://github.com/chgl/fhir-server-exporter/commit/b1afb530125bab04812fac88a5f2ddd76b093e8e))

## [3.0.1](https://github.com/chgl/fhir-server-exporter/compare/v3.0.0...v3.0.1) (2026-01-17)


### Miscellaneous Chores

* **deps:** update dependency fakeiteasy to v9 ([#446](https://github.com/chgl/fhir-server-exporter/issues/446)) ([70a2a19](https://github.com/chgl/fhir-server-exporter/commit/70a2a19d9dbb00fed88d739b45bdf77e629b3314))
* **deps:** update docker.io/library/eclipse-temurin:21.0.7_6-jre-ubi9-minimal docker digest to be37f42 ([#450](https://github.com/chgl/fhir-server-exporter/issues/450)) ([0d0458d](https://github.com/chgl/fhir-server-exporter/commit/0d0458dc6afa983f16946321b2043c240f551ba3))
* **deps:** update github-actions ([#445](https://github.com/chgl/fhir-server-exporter/issues/445)) ([1d5f689](https://github.com/chgl/fhir-server-exporter/commit/1d5f68975388e9608987569e2a94918bac8df6b5))
* **deps:** update mcr.microsoft.com/dotnet/sdk:10.0.101-noble docker digest to 5504edd ([#447](https://github.com/chgl/fhir-server-exporter/issues/447)) ([537f771](https://github.com/chgl/fhir-server-exporter/commit/537f771dee4de234b69b7c490966b842ba72e1f9))
* install aws extensions in container ([cf67424](https://github.com/chgl/fhir-server-exporter/commit/cf6742490d486a6c3d1ca649b0a20fbb793a2c00))

## [3.0.0](https://github.com/chgl/fhir-server-exporter/compare/v2.3.50...v3.0.0) (2026-01-17)


### ⚠ BREAKING CHANGES

* removed support for custom queries for now.

### Features

* added support for querying FHIR lakehouses in Pathling-compatible layout ([#449](https://github.com/chgl/fhir-server-exporter/issues/449)) ([31412a8](https://github.com/chgl/fhir-server-exporter/commit/31412a8b5fa7d0bc5899f3270d7a0c19f00c67e4))


### Miscellaneous Chores

* **deps:** update chgl/.github action to v1.11.11 ([#443](https://github.com/chgl/fhir-server-exporter/issues/443)) ([7d22069](https://github.com/chgl/fhir-server-exporter/commit/7d220691a5f8372df92ba1d10c23ac0fd84be0a5))

## [2.3.50](https://github.com/chgl/fhir-server-exporter/compare/v2.3.49...v2.3.50) (2025-12-18)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#435](https://github.com/chgl/fhir-server-exporter/issues/435)) ([c2426d8](https://github.com/chgl/fhir-server-exporter/commit/c2426d8debd708fd9283b5f2e39e80d3e10519eb))
* **deps:** update chgl/.github action to v1.11.10 ([#436](https://github.com/chgl/fhir-server-exporter/issues/436)) ([eec0af0](https://github.com/chgl/fhir-server-exporter/commit/eec0af06c2061517db1764ba14502a9dba07a6e3))
* **deps:** update dependency meziantou.analyzer to 2.0.261 ([#439](https://github.com/chgl/fhir-server-exporter/issues/439)) ([54a46d1](https://github.com/chgl/fhir-server-exporter/commit/54a46d1ac0c70468fe5f17a2f8db044712532e24))
* **deps:** update dependency meziantou.analyzer to 2.0.263 ([#441](https://github.com/chgl/fhir-server-exporter/issues/441)) ([8eee829](https://github.com/chgl/fhir-server-exporter/commit/8eee82979184bae491bbfecd8f1147c0f53a6afc))
* **deps:** update dotnet monorepo to v10 (major) ([#420](https://github.com/chgl/fhir-server-exporter/issues/420)) ([394ab37](https://github.com/chgl/fhir-server-exporter/commit/394ab37b25ea5c71a16a7358b057e428465781c3))
* **deps:** update github-actions ([#434](https://github.com/chgl/fhir-server-exporter/issues/434)) ([7278158](https://github.com/chgl/fhir-server-exporter/commit/7278158150d5c68d2f236f7f5726387e4bb1280d))
* **deps:** update github/codeql-action action to v4.31.9 ([#438](https://github.com/chgl/fhir-server-exporter/issues/438)) ([1615571](https://github.com/chgl/fhir-server-exporter/commit/16155719b0631307f4b835a0329b5072d75029aa))
* **deps:** updated duende access token manangement to latest ([#440](https://github.com/chgl/fhir-server-exporter/issues/440)) ([b26a449](https://github.com/chgl/fhir-server-exporter/commit/b26a44942b0f3fb894c4aa0050b99bc8f97176ca))

## [2.3.49](https://github.com/chgl/fhir-server-exporter/compare/v2.3.48...v2.3.49) (2025-12-12)


### Miscellaneous Chores

* **deps:** update actions/checkout action to v6 ([#426](https://github.com/chgl/fhir-server-exporter/issues/426)) ([a4704de](https://github.com/chgl/fhir-server-exporter/commit/a4704dedf8ecb8fbaae8c3a71d515e501cf9f535))
* **deps:** update all non-major dependencies ([#427](https://github.com/chgl/fhir-server-exporter/issues/427)) ([bd7e0ab](https://github.com/chgl/fhir-server-exporter/commit/bd7e0ab314e17e229383cf28c5a72931b31bdbd1))
* **deps:** update all non-major dependencies ([#430](https://github.com/chgl/fhir-server-exporter/issues/430)) ([17767b1](https://github.com/chgl/fhir-server-exporter/commit/17767b113269d173de890d5aa0cebfa7b4036db1))
* **deps:** update dotnet monorepo ([#432](https://github.com/chgl/fhir-server-exporter/issues/432)) ([92663bd](https://github.com/chgl/fhir-server-exporter/commit/92663bd0c7e24656fc2828064da3da354171d0e7))
* **deps:** update github-actions ([#428](https://github.com/chgl/fhir-server-exporter/issues/428)) ([f150fc1](https://github.com/chgl/fhir-server-exporter/commit/f150fc1d1ad92758ccda4ae34385f097a3e59d61))
* **deps:** update github-actions ([#431](https://github.com/chgl/fhir-server-exporter/issues/431)) ([7168cce](https://github.com/chgl/fhir-server-exporter/commit/7168cceeae7f6dc931f6e699cf783620722af827))
* **deps:** update github-actions ([#433](https://github.com/chgl/fhir-server-exporter/issues/433)) ([5497765](https://github.com/chgl/fhir-server-exporter/commit/5497765746c8236124e31bd0b33f75075c737196))

## [2.3.48](https://github.com/chgl/fhir-server-exporter/compare/v2.3.47...v2.3.48) (2025-11-19)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#410](https://github.com/chgl/fhir-server-exporter/issues/410)) ([445955b](https://github.com/chgl/fhir-server-exporter/commit/445955baa72514d6db2b8ce9c99f8f12049da40b))
* **deps:** update all non-major dependencies ([#415](https://github.com/chgl/fhir-server-exporter/issues/415)) ([08a42aa](https://github.com/chgl/fhir-server-exporter/commit/08a42aa3cf6f0106422cd97652b120ecbc0df1c9))
* **deps:** update all non-major dependencies ([#416](https://github.com/chgl/fhir-server-exporter/issues/416)) ([28228e3](https://github.com/chgl/fhir-server-exporter/commit/28228e30637320cf114115188fa133ac85f0e49c))
* **deps:** update all non-major dependencies ([#418](https://github.com/chgl/fhir-server-exporter/issues/418)) ([e05c98d](https://github.com/chgl/fhir-server-exporter/commit/e05c98d6f215e214fdd758ad78a48337175b03f6))
* **deps:** update chgl/.github action to v1.11.5 ([#419](https://github.com/chgl/fhir-server-exporter/issues/419)) ([c0458d5](https://github.com/chgl/fhir-server-exporter/commit/c0458d579e9353832dde0e38b9faf97e7945627b))
* **deps:** update dependency meziantou.analyzer to 2.0.237 ([#412](https://github.com/chgl/fhir-server-exporter/issues/412)) ([bf5894e](https://github.com/chgl/fhir-server-exporter/commit/bf5894e0f520acbb33e3bafbc571fd6b3e83ec28))
* **deps:** update dependency meziantou.analyzer to 2.0.238 ([#413](https://github.com/chgl/fhir-server-exporter/issues/413)) ([7564d64](https://github.com/chgl/fhir-server-exporter/commit/7564d6419b992c0f5c0414b90bc33ef0a32a2490))
* **deps:** update dependency meziantou.analyzer to 2.0.254 ([#423](https://github.com/chgl/fhir-server-exporter/issues/423)) ([b7dad59](https://github.com/chgl/fhir-server-exporter/commit/b7dad59f1a5d5b81db22f8f3d049b2faa29a8b49))
* **deps:** update dependency testcontainers to 4.8.0 ([#408](https://github.com/chgl/fhir-server-exporter/issues/408)) ([a0cb3d5](https://github.com/chgl/fhir-server-exporter/commit/a0cb3d5abacda363e7eed91d4361b0c362aabd07))
* **deps:** update github-actions ([#407](https://github.com/chgl/fhir-server-exporter/issues/407)) ([87509df](https://github.com/chgl/fhir-server-exporter/commit/87509dfdef0344808e9a5918eee0c3dd11b96e2a))
* **deps:** update github-actions ([#411](https://github.com/chgl/fhir-server-exporter/issues/411)) ([7ab067a](https://github.com/chgl/fhir-server-exporter/commit/7ab067a2e8ea1af5ee501fcc9b59036305361fea))
* **deps:** update github-actions ([#414](https://github.com/chgl/fhir-server-exporter/issues/414)) ([98da072](https://github.com/chgl/fhir-server-exporter/commit/98da0728a403d3b84b62b233a95c426ee19e2b95))
* **deps:** update github-actions ([#422](https://github.com/chgl/fhir-server-exporter/issues/422)) ([31d24c0](https://github.com/chgl/fhir-server-exporter/commit/31d24c06a464b6a96110ac9a1032be9544ba332f))
* **deps:** update mcr.microsoft.com/dotnet/aspnet:9.0.11-noble-chiseled docker digest to 2100e4a ([#424](https://github.com/chgl/fhir-server-exporter/issues/424)) ([f661c49](https://github.com/chgl/fhir-server-exporter/commit/f661c491c9755ba7c85bef2b2de5bc8ec5bfdeca))
* **deps:** update mcr.microsoft.com/dotnet/sdk docker tag to v9.0.308 ([#425](https://github.com/chgl/fhir-server-exporter/issues/425)) ([347d682](https://github.com/chgl/fhir-server-exporter/commit/347d682042f4ad4632565c02b6970b91ee4432e7))
* **deps:** update step-security/harden-runner action to v2.13.2 ([#417](https://github.com/chgl/fhir-server-exporter/issues/417)) ([13d83a4](https://github.com/chgl/fhir-server-exporter/commit/13d83a4dd78ca41ffb1cebfdad0f942e6a128855))

## [2.3.47](https://github.com/chgl/fhir-server-exporter/compare/v2.3.46...v2.3.47) (2025-10-17)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#382](https://github.com/chgl/fhir-server-exporter/issues/382)) ([d64a03d](https://github.com/chgl/fhir-server-exporter/commit/d64a03ddd0f0283994715ea5c3d7fbce0704a38c))
* **deps:** update all non-major dependencies ([#388](https://github.com/chgl/fhir-server-exporter/issues/388)) ([2c69dde](https://github.com/chgl/fhir-server-exporter/commit/2c69ddeabb394a30c8c06f7db157321f39813184))
* **deps:** update all non-major dependencies ([#392](https://github.com/chgl/fhir-server-exporter/issues/392)) ([15b39fd](https://github.com/chgl/fhir-server-exporter/commit/15b39fdc4c24fc2919e4ca39fa26725b19729d16))
* **deps:** update all non-major dependencies ([#394](https://github.com/chgl/fhir-server-exporter/issues/394)) ([4c86ef7](https://github.com/chgl/fhir-server-exporter/commit/4c86ef77a28709b11809d9e0a621eaeb7949f3e5))
* **deps:** update all non-major dependencies ([#401](https://github.com/chgl/fhir-server-exporter/issues/401)) ([95a6239](https://github.com/chgl/fhir-server-exporter/commit/95a6239b74998ddb7ebab39bdc04794605096379))
* **deps:** update all non-major dependencies ([#403](https://github.com/chgl/fhir-server-exporter/issues/403)) ([72a55f4](https://github.com/chgl/fhir-server-exporter/commit/72a55f44926c81fca99c332f5086e167e65a6089))
* **deps:** update all non-major dependencies ([#404](https://github.com/chgl/fhir-server-exporter/issues/404)) ([c7c995e](https://github.com/chgl/fhir-server-exporter/commit/c7c995ec95dda4a3437e1b965363d87f0156259b))
* **deps:** update all non-major dependencies to 9.0.9 ([#379](https://github.com/chgl/fhir-server-exporter/issues/379)) ([fa46cce](https://github.com/chgl/fhir-server-exporter/commit/fa46cce38dde6b966480c3dbd49a68cf9fff19d6))
* **deps:** update dependency hl7.fhir.r4 to v6 ([#402](https://github.com/chgl/fhir-server-exporter/issues/402)) ([b8e8014](https://github.com/chgl/fhir-server-exporter/commit/b8e801490aecc9dc9f6f033ec624a34760099c8a))
* **deps:** update dependency meziantou.analyzer to 2.0.218 ([#384](https://github.com/chgl/fhir-server-exporter/issues/384)) ([f875fbc](https://github.com/chgl/fhir-server-exporter/commit/f875fbc5b467bf9fd04f4b8d23642c73c02c8470))
* **deps:** update dependency meziantou.analyzer to 2.0.221 ([#391](https://github.com/chgl/fhir-server-exporter/issues/391)) ([7a7ee42](https://github.com/chgl/fhir-server-exporter/commit/7a7ee420b94e5485603c3e7d677db10feea0fd97))
* **deps:** update dependency microsoft.net.test.sdk to v18 ([#397](https://github.com/chgl/fhir-server-exporter/issues/397)) ([4ecc297](https://github.com/chgl/fhir-server-exporter/commit/4ecc29713c53c5524baf8381fd4c46020c5b1aa0))
* **deps:** update github-actions ([#389](https://github.com/chgl/fhir-server-exporter/issues/389)) ([d9b7bf3](https://github.com/chgl/fhir-server-exporter/commit/d9b7bf36c55bbff9a4c6f7c7fb8714d2ece172e0))
* **deps:** update github-actions ([#393](https://github.com/chgl/fhir-server-exporter/issues/393)) ([07582d3](https://github.com/chgl/fhir-server-exporter/commit/07582d37d5197376c20ee0f47762533f98939843))
* **deps:** update github-actions ([#395](https://github.com/chgl/fhir-server-exporter/issues/395)) ([a608b2f](https://github.com/chgl/fhir-server-exporter/commit/a608b2f51c64831a313b6911ca96d6798b97ba3e))
* **deps:** update github-actions ([#398](https://github.com/chgl/fhir-server-exporter/issues/398)) ([68acf7d](https://github.com/chgl/fhir-server-exporter/commit/68acf7dc49942a28dc901133a6a6a6e4a8c5ada3))
* **deps:** update github-actions ([#399](https://github.com/chgl/fhir-server-exporter/issues/399)) ([2f2222c](https://github.com/chgl/fhir-server-exporter/commit/2f2222c65ff6e3aa09a474cd9c6529178e0d8cc2))
* **deps:** update github/codeql-action action to v3.30.2 ([#380](https://github.com/chgl/fhir-server-exporter/issues/380)) ([c97f408](https://github.com/chgl/fhir-server-exporter/commit/c97f408850c6d933a0842a839d89301922b3eb4b))
* **deps:** update github/codeql-action action to v3.30.3 ([#387](https://github.com/chgl/fhir-server-exporter/issues/387)) ([7768e9b](https://github.com/chgl/fhir-server-exporter/commit/7768e9b38f895754243880027c5c28f29db9d214))
* **deps:** update github/codeql-action action to v4 ([#400](https://github.com/chgl/fhir-server-exporter/issues/400)) ([e8db652](https://github.com/chgl/fhir-server-exporter/commit/e8db65260e442d19be6bf6b1f45f06eb8f0f3a7e))
* **deps:** update github/codeql-action action to v4.30.9 ([#406](https://github.com/chgl/fhir-server-exporter/issues/406)) ([9e8d619](https://github.com/chgl/fhir-server-exporter/commit/9e8d61961cbe3d29ee72449cb5313465519234ba))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.305-noble docker digest to 604ef06 ([#390](https://github.com/chgl/fhir-server-exporter/issues/390)) ([58124d5](https://github.com/chgl/fhir-server-exporter/commit/58124d5df2d9205d0a3c23bdf02c3704ac6c77a9))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.305-noble docker digest to 802e64a ([#385](https://github.com/chgl/fhir-server-exporter/issues/385)) ([8fc9d32](https://github.com/chgl/fhir-server-exporter/commit/8fc9d32984341782dabd75167a3a3a2423ab45ea))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.305-noble docker digest to 9ae2f68 ([#396](https://github.com/chgl/fhir-server-exporter/issues/396)) ([b7a0ab8](https://github.com/chgl/fhir-server-exporter/commit/b7a0ab8d08bc59316963441c72bdc462fb41971d))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.306-noble docker digest to d88e637 ([#405](https://github.com/chgl/fhir-server-exporter/issues/405)) ([c6d89f3](https://github.com/chgl/fhir-server-exporter/commit/c6d89f3b907b429fe191bc5dda26ad5a45d97a1d))
* **deps:** update step-security/harden-runner action to v2.13.1 ([#383](https://github.com/chgl/fhir-server-exporter/issues/383)) ([450e79f](https://github.com/chgl/fhir-server-exporter/commit/450e79fea76cacbf1dd303d885c60bd6b662bae7))

## [2.3.46](https://github.com/chgl/fhir-server-exporter/compare/v2.3.45...v2.3.46) (2025-09-09)


### Miscellaneous Chores

* add .whitesource configuration file ([#372](https://github.com/chgl/fhir-server-exporter/issues/372)) ([0a0ce03](https://github.com/chgl/fhir-server-exporter/commit/0a0ce03d67d53304c4cf8d617ba4fef57392ae1a))
* **deps:** update actions/setup-dotnet action to v5 ([#378](https://github.com/chgl/fhir-server-exporter/issues/378)) ([666df3c](https://github.com/chgl/fhir-server-exporter/commit/666df3c47c856121f910538e25e206f69f2fbb4a))
* **deps:** update all non-major dependencies ([#373](https://github.com/chgl/fhir-server-exporter/issues/373)) ([9972200](https://github.com/chgl/fhir-server-exporter/commit/99722001433bfc20d55e454bf1229eaa90b5605e))
* **deps:** update aquasecurity/trivy-action action to v0.33.0 ([#374](https://github.com/chgl/fhir-server-exporter/issues/374)) ([9c34698](https://github.com/chgl/fhir-server-exporter/commit/9c346980f8b3c42d5483265048fc104d090f84ae))
* **deps:** update dependency meziantou.analyzer to 2.0.215 ([#376](https://github.com/chgl/fhir-server-exporter/issues/376)) ([2512c11](https://github.com/chgl/fhir-server-exporter/commit/2512c11918a0c1557b678d3cdb03fb77315003db))
* **deps:** update dependency testcontainers to 4.7.0 ([#375](https://github.com/chgl/fhir-server-exporter/issues/375)) ([194d348](https://github.com/chgl/fhir-server-exporter/commit/194d34891d4ed669583182009f1f4044f8d4e08a))
* **deps:** update github-actions ([#368](https://github.com/chgl/fhir-server-exporter/issues/368)) ([0fb5c1a](https://github.com/chgl/fhir-server-exporter/commit/0fb5c1a1390a99fe6070403b95729973e242d12a))
* **deps:** update github-actions ([#377](https://github.com/chgl/fhir-server-exporter/issues/377)) ([5603e4f](https://github.com/chgl/fhir-server-exporter/commit/5603e4fb2d556ee899e4c0500b06e22fef1293af))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.304-noble docker digest to 0b7186a ([#369](https://github.com/chgl/fhir-server-exporter/issues/369)) ([fd2017e](https://github.com/chgl/fhir-server-exporter/commit/fd2017eb238fde24a4d630cd72486d51f25add9f))
* **deps:** update quay.io/keycloak/keycloak docker tag to v26.3.3 ([#370](https://github.com/chgl/fhir-server-exporter/issues/370)) ([2290d9a](https://github.com/chgl/fhir-server-exporter/commit/2290d9a0093cb811578cc461c025554e20e28c15))

## [2.3.45](https://github.com/chgl/fhir-server-exporter/compare/v2.3.44...v2.3.45) (2025-08-16)


### Miscellaneous Chores

* **deps:** update actions/checkout action to v5 ([#365](https://github.com/chgl/fhir-server-exporter/issues/365)) ([b7c120a](https://github.com/chgl/fhir-server-exporter/commit/b7c120ade653cbb67b9076fa0cd23646052b137b))
* **deps:** update all non-major dependencies ([#363](https://github.com/chgl/fhir-server-exporter/issues/363)) ([ea1c76e](https://github.com/chgl/fhir-server-exporter/commit/ea1c76eeb69ce6675591f0ebbd3389a136da3c4a))
* **deps:** update github-actions ([#364](https://github.com/chgl/fhir-server-exporter/issues/364)) ([5fb7708](https://github.com/chgl/fhir-server-exporter/commit/5fb7708111417889b356bb5e468cd1ca9674bc99))
* **deps:** update quay.io/keycloak/keycloak:26.3.2 docker digest to 98fab02 ([#366](https://github.com/chgl/fhir-server-exporter/issues/366)) ([1db1312](https://github.com/chgl/fhir-server-exporter/commit/1db1312960a86dbc0059cec16a64cfee6014ea71))

## [2.3.44](https://github.com/chgl/fhir-server-exporter/compare/v2.3.43...v2.3.44) (2025-08-06)


### Miscellaneous Chores

* **deps:** update actions/download-artifact action to v5 ([#362](https://github.com/chgl/fhir-server-exporter/issues/362)) ([28374e8](https://github.com/chgl/fhir-server-exporter/commit/28374e868255722dd269d7a29f22c4ac9bdf0b67))
* **deps:** update all non-major dependencies ([#357](https://github.com/chgl/fhir-server-exporter/issues/357)) ([c6a47dc](https://github.com/chgl/fhir-server-exporter/commit/c6a47dc33ab397768311d99fcee19235495b3bfd))
* **deps:** update all non-major dependencies ([#360](https://github.com/chgl/fhir-server-exporter/issues/360)) ([d30b249](https://github.com/chgl/fhir-server-exporter/commit/d30b24905eb74bbae3cae9a3a45b20fd9e25f4b1))
* **deps:** update dependency csharpier to 1.1.1 ([#359](https://github.com/chgl/fhir-server-exporter/issues/359)) ([9a245f5](https://github.com/chgl/fhir-server-exporter/commit/9a245f50589a21d5557af11adfb8a9c6cdd16425))
* **deps:** update dependency meziantou.analyzer to 2.0.207 ([#355](https://github.com/chgl/fhir-server-exporter/issues/355)) ([aee65d6](https://github.com/chgl/fhir-server-exporter/commit/aee65d678415d00a4365ce31b569b0c87a008830))
* **deps:** update dotnet monorepo ([#361](https://github.com/chgl/fhir-server-exporter/issues/361)) ([a78e3a3](https://github.com/chgl/fhir-server-exporter/commit/a78e3a3d19a183bef1992240bc86976ff3ca398d))
* **deps:** update github-actions ([#358](https://github.com/chgl/fhir-server-exporter/issues/358)) ([ec37d5b](https://github.com/chgl/fhir-server-exporter/commit/ec37d5b72b22367a50735c7b562d5fa820c43a41))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.303-noble docker digest to 14fad15 ([#349](https://github.com/chgl/fhir-server-exporter/issues/349)) ([aa792d0](https://github.com/chgl/fhir-server-exporter/commit/aa792d0140d85d62f37d4aa36646d9b0efed60e2))

## [2.3.43](https://github.com/chgl/fhir-server-exporter/compare/v2.3.42...v2.3.43) (2025-07-26)


### Miscellaneous Chores

* **deps:** update dependency meziantou.analyzer to 2.0.206 ([#353](https://github.com/chgl/fhir-server-exporter/issues/353)) ([5a8f5fc](https://github.com/chgl/fhir-server-exporter/commit/5a8f5fcf2204c20fe5d91797eb9df9d9e83f3877))
* **deps:** update github-actions ([#350](https://github.com/chgl/fhir-server-exporter/issues/350)) ([e29e36b](https://github.com/chgl/fhir-server-exporter/commit/e29e36bea1fa7d6b622349451a923b1c9e2f10a1))

## [2.3.42](https://github.com/chgl/fhir-server-exporter/compare/v2.3.41...v2.3.42) (2025-07-24)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#351](https://github.com/chgl/fhir-server-exporter/issues/351)) ([58f5924](https://github.com/chgl/fhir-server-exporter/commit/58f592489537cabdb7d7e834c0aa1747354d6957))

## [2.3.41](https://github.com/chgl/fhir-server-exporter/compare/v2.3.40...v2.3.41) (2025-06-15)


### Miscellaneous Chores

* **deps:** update all non-major dependencies to v8.14.1 ([#348](https://github.com/chgl/fhir-server-exporter/issues/348)) ([15a3b71](https://github.com/chgl/fhir-server-exporter/commit/15a3b71e416c507f5676d5feb68126f4371dad5b))
* **deps:** update chgl/.github action to v1.10.41 ([#347](https://github.com/chgl/fhir-server-exporter/issues/347)) ([b4d4f11](https://github.com/chgl/fhir-server-exporter/commit/b4d4f11724fd7674589144f796860dd6b679cd19))
* **deps:** update dependency meziantou.analyzer to 2.0.202 ([#345](https://github.com/chgl/fhir-server-exporter/issues/345)) ([6e14192](https://github.com/chgl/fhir-server-exporter/commit/6e1419206de66d8d00837da56ceb52507ce535af))

## [2.3.40](https://github.com/chgl/fhir-server-exporter/compare/v2.3.39...v2.3.40) (2025-06-14)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#343](https://github.com/chgl/fhir-server-exporter/issues/343)) ([e6a056c](https://github.com/chgl/fhir-server-exporter/commit/e6a056c1f246373bea63d2a426432e53e59363ea))
* **deps:** update dependency testcontainers to 4.5.0 ([#341](https://github.com/chgl/fhir-server-exporter/issues/341)) ([da19bd1](https://github.com/chgl/fhir-server-exporter/commit/da19bd12bf357c8990439ff2e74351dab76ea206))
* **deps:** update github-actions ([#344](https://github.com/chgl/fhir-server-exporter/issues/344)) ([e9bad4a](https://github.com/chgl/fhir-server-exporter/commit/e9bad4afdd5a89cde85c761fc1d25aada46bb695))

## [2.3.39](https://github.com/chgl/fhir-server-exporter/compare/v2.3.38...v2.3.39) (2025-06-03)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#338](https://github.com/chgl/fhir-server-exporter/issues/338)) ([7a865da](https://github.com/chgl/fhir-server-exporter/commit/7a865da5277262ee670639619892aa189ed1e1e1))
* **deps:** update github-actions ([#340](https://github.com/chgl/fhir-server-exporter/issues/340)) ([a8d5701](https://github.com/chgl/fhir-server-exporter/commit/a8d570150b0c86bd65133c244ee7a1e3dbfa3d2a))
* **deps:** update ossf/scorecard-action action to v2.4.2 ([#337](https://github.com/chgl/fhir-server-exporter/issues/337)) ([3fcf26e](https://github.com/chgl/fhir-server-exporter/commit/3fcf26e337e965a517b40690cbddaa4cf051a03f))

## [2.3.38](https://github.com/chgl/fhir-server-exporter/compare/v2.3.37...v2.3.38) (2025-05-29)


### Miscellaneous Chores

* **deps:** update chgl/.github action to v1.10.39 ([#335](https://github.com/chgl/fhir-server-exporter/issues/335)) ([bc9e2d9](https://github.com/chgl/fhir-server-exporter/commit/bc9e2d9783d55bf1bd9bbc0f830ef376b30f0814))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.300-noble docker digest to 9f7bd4d ([#334](https://github.com/chgl/fhir-server-exporter/issues/334)) ([4fcd888](https://github.com/chgl/fhir-server-exporter/commit/4fcd88825b96cdd5582e0a7a3174e76096fb3a73))

## [2.3.37](https://github.com/chgl/fhir-server-exporter/compare/v2.3.36...v2.3.37) (2025-05-29)


### Miscellaneous Chores

* **deps:** update all non-major dependencies ([#330](https://github.com/chgl/fhir-server-exporter/issues/330)) ([fb9174c](https://github.com/chgl/fhir-server-exporter/commit/fb9174c334db17c1a71aa67128cf5fb81a98c75a))

## [2.3.36](https://github.com/chgl/fhir-server-exporter/compare/v2.3.35...v2.3.36) (2025-05-29)


### Miscellaneous Chores

* **config:** migrate renovate config ([#325](https://github.com/chgl/fhir-server-exporter/issues/325)) ([a79c323](https://github.com/chgl/fhir-server-exporter/commit/a79c32328a30ce633d7e40feb277c7551345adc1))
* **deps:** update all non-major dependencies ([#313](https://github.com/chgl/fhir-server-exporter/issues/313)) ([4fe2daa](https://github.com/chgl/fhir-server-exporter/commit/4fe2daa8bf28297f197a8aeb63f1163d53e259ab))
* **deps:** update all non-major dependencies ([#316](https://github.com/chgl/fhir-server-exporter/issues/316)) ([1a2426f](https://github.com/chgl/fhir-server-exporter/commit/1a2426f3621afced219fed3a4855f42b9f2e1f00))
* **deps:** update all non-major dependencies ([#318](https://github.com/chgl/fhir-server-exporter/issues/318)) ([f1f1cf7](https://github.com/chgl/fhir-server-exporter/commit/f1f1cf7f21e1385749f4c6bf53f957bbc4b8eee4))
* **deps:** update all non-major dependencies ([#323](https://github.com/chgl/fhir-server-exporter/issues/323)) ([52699bf](https://github.com/chgl/fhir-server-exporter/commit/52699bf4f0bb463ec74a024b30b4d6bd7100641b))
* **deps:** update all non-major dependencies ([#324](https://github.com/chgl/fhir-server-exporter/issues/324)) ([a6c80fc](https://github.com/chgl/fhir-server-exporter/commit/a6c80fc0538e52cc2333008384652be39030f541))
* **deps:** update all non-major dependencies ([#328](https://github.com/chgl/fhir-server-exporter/issues/328)) ([c69dfc8](https://github.com/chgl/fhir-server-exporter/commit/c69dfc8b3319a7def2f5f5edd8d5c08634093b00))
* **deps:** update chgl/.github action to v1.10.25 ([#317](https://github.com/chgl/fhir-server-exporter/issues/317)) ([0f9a797](https://github.com/chgl/fhir-server-exporter/commit/0f9a79733ed717aba344c28a8031eb22d050e66b))
* **deps:** update chgl/.github action to v1.10.38 ([#329](https://github.com/chgl/fhir-server-exporter/issues/329)) ([d7459d8](https://github.com/chgl/fhir-server-exporter/commit/d7459d84b9482cc137fd5390318008b6f1077e0a))
* **deps:** update dependency csharpier to v1 ([#320](https://github.com/chgl/fhir-server-exporter/issues/320)) ([87584f6](https://github.com/chgl/fhir-server-exporter/commit/87584f6bc5d33d380093d8601c1071b511dfd3f0))
* **deps:** update github-actions ([#314](https://github.com/chgl/fhir-server-exporter/issues/314)) ([f3f35b5](https://github.com/chgl/fhir-server-exporter/commit/f3f35b5da0c981947a3bc473485739c323dc9150))
* **deps:** update github-actions ([#319](https://github.com/chgl/fhir-server-exporter/issues/319)) ([e3cc89e](https://github.com/chgl/fhir-server-exporter/commit/e3cc89eb3470f7936833eccca4068b58052643d5))
* **deps:** update github-actions ([#321](https://github.com/chgl/fhir-server-exporter/issues/321)) ([fde09eb](https://github.com/chgl/fhir-server-exporter/commit/fde09ebc6417b60005ad12e802c9ddd23fc5ed75))
* **deps:** update github-actions ([#326](https://github.com/chgl/fhir-server-exporter/issues/326)) ([cd82c23](https://github.com/chgl/fhir-server-exporter/commit/cd82c233126b02a04e7a3fb34c5f8f674554fe6f))
* **deps:** update github/codeql-action action to v3.28.15 ([#315](https://github.com/chgl/fhir-server-exporter/issues/315)) ([8ab2c28](https://github.com/chgl/fhir-server-exporter/commit/8ab2c28186bd86e5a99286b2722d37070ca56566))
* **deps:** update mcr.microsoft.com/dotnet/sdk:9.0.203-noble docker digest to c849687 ([#322](https://github.com/chgl/fhir-server-exporter/issues/322)) ([926bc3a](https://github.com/chgl/fhir-server-exporter/commit/926bc3a0949871cc2b45f86542714b8641dfb892))


### CI/CD

* switch to release please ([#331](https://github.com/chgl/fhir-server-exporter/issues/331)) ([7e43bc0](https://github.com/chgl/fhir-server-exporter/commit/7e43bc025828cb600d4cd001faff1b75644a29b7))
* switch to releaser app workflow ([#327](https://github.com/chgl/fhir-server-exporter/issues/327)) ([448ac02](https://github.com/chgl/fhir-server-exporter/commit/448ac0226faacad3c80ba0ea44052d5c24e9383b))
