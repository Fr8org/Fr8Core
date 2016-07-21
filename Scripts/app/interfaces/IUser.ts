/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IUserDTO extends ng.resource.IResource<any> {
        id: string;
        firstName: string;
        lastName: string;
        userName: string;
        email: string;
        status: string;
        role: string;
        organizationId: number;
        profileId: string;
        class: string;
        canManagePageDefinitions: boolean;
    }
}