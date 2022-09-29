# .Net Core Semantic Release

## Conventional Commit

參考網站: https://www.conventionalcommits.org/en/v1.0.0/

在 Commit 時按照預先定義好的格式加入固定的 Prefix, 發布版本的時候就可以自動根據 Commit Message 來自動產生適合的版號，搭配 Semantic Release 就可以讓版號能夠正確的 Library 異動的範圍。

## Semantic Release

參考網站: https://semantic-release.gitbook.io/semantic-release/

第三方 Node.js Library，搭配 Convential Commit 使用，用來自動產生 Library 的版號，可以自動判斷需要 Bump 的版號範圍，來確保 Library 的版號有遵守 [Semantic versioning](https://semver.org/) 的規範。

本專案使用 Cake 來執行 Semantic Release，並在版本發布時自動更新 Library 的版號以及包裝新的 Nuget Package，細節設定可參考 build.cake。

Semantic Release 專案設定在 .releaserc 檔案之中。

## Husky.Net

參考網站: https://alirezanet.github.io/Husky.Net/

.Net 版的 Git hook 工具，可以在 Commit 之前自動檢查 Commit Message 是否符合 Conventional Commit 的規範，或是可以在 Git 的各種生命週期事件發生之前進行動作（例如 Linting），來確保不會有錯誤的訊息被 Commit 進去。

專案設定可參考 .husky/task-runner.json。
