namespace TooYoung.Domain.Services
open TooYoung.Domain.Repositories

type SharingService (repo: ISharingRepository) =
    let startWork = Repository.startWork repo
    let unitwork = Repository.unitWork repo

    

