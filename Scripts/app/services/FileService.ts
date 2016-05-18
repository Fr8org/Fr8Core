module dockyard.services {

    export interface IFileService {
        uploadFile(file: any): any;
        listFiles(): ng.IPromise<Array<interfaces.IFileDescriptionDTO>>;
    }
    /*
        General data persistance methods for FileDirective.
    */
     export class FileService implements IFileService {
        constructor(
            private $http: ng.IHttpService,
            private $q: ng.IQService,
            private UploadService: any
        ) { }


        public uploadFile(file: any): any {
            var deferred = this.$q.defer();

            this.UploadService.upload({
                url: '/api/files',
                file: file
            }).progress((event: any) => {
                console.log('Loaded: ' + event.loaded + ' / ' + event.total);
            })
                .success((fileDTO: interfaces.IFileDescriptionDTO) => {
                    deferred.resolve(fileDTO);
                })
                .error((data: any, status: any) => {
                    deferred.reject(status);
                });

            return deferred.promise;
        }

        public listFiles(): ng.IPromise<Array<interfaces.IFileDescriptionDTO>> {
            var deferred = this.$q.defer();
            this.$http.get<Array<interfaces.IFileDescriptionDTO>>('/api/files/list').then(resp => {
                deferred.resolve(resp.data);
            }, err => {
                deferred.reject(err);
            });
            return deferred.promise;

        }
    }

    /*
        Register FileService with AngularJS. Upload dependency comes from ng-file-upload module
    */
    app.factory('FileService', ['$http', '$q', 'Upload',
        ($http, $q, UploadService) => {
            return new FileService($http, $q, UploadService);
    }]);
}